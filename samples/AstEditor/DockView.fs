namespace AstEditor

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Presenters
open Avalonia.Controls.Templates
open Avalonia.VisualTree
open Dock.Model.Core
open Dock.Model.Core.Events
open Dock.Model.Mvvm
open Dock.Model.Mvvm.Controls
open Dock.Avalonia.Controls
open Fabulous
open Fabulous.ScalarAttributeDefinitions
open Fabulous.Avalonia

// Dock's DockControl. Aliased to avoid clashing with the `DockControl` module below.
type private DockCtl = Dock.Avalonia.Controls.DockControl

/// A Fabulous.Avalonia binding for Dock's DockControl. It hosts the IDE layout: a row of DSL
/// editor *tabs* (one Document per sample) beside the generated-F# pane, with the output
/// console docked below — all live Fabulous-materialized controls.
///
/// To make the arrangement persistable, each Document carries a stable Id and its content is
/// resolved by Id (from a per-control registry) via a DataTemplate — so the *structure*
/// serializes cleanly while the live controls re-bind on load.
type IFabDockControl =
    inherit IFabControl

module private Ids =
    let dsl i = $"dsl-%d{i}"
    let generated = "generated"
    let syntaxTree = "syntaxtree"
    let rewrite = "rewrite"
    let output = "output"

    let tryDslIndex(id: string) =
        if not(isNull id) && id.StartsWith("dsl-", StringComparison.Ordinal) then
            match Int32.TryParse(id.Substring 4) with
            | true, i -> Some i
            | _ -> None
        else
            None

[<AutoOpen>]
module private DockInterop =

    let mutable private stylesAdded = false

    let ensureDockStyles() =
        if not stylesAdded then
            match Application.Current with
            | null -> ()
            | app ->
                stylesAdded <- true

                try
                    app.Styles.Add(Dock.Avalonia.Themes.Fluent.DockFluentTheme())
                with ex ->
                    eprintfn "AstEditor: failed to load Dock theme — %s" ex.Message

    /// Per-DockControl state: the live pane controls (by document Id), the tab titles, the
    /// active-tab dispatcher, and whether the layout is built.
    type Panes() =
        member val Controls = Dictionary<string, Control>() with get
        member val Names: string[] = null with get, set
        member val Built = false with get, set
        member val ActivateFn: (int -> unit) option = None with get, set
        /// Set after the build, so closed tabs can be re-inserted into the live layout.
        member val Factory: Factory option = None with get, set
        member val DslDock: DocumentDock = null with get, set
        member val DslDocs: Document[] = [||] with get, set
        member val CloseFn: (int -> unit) option = None with get, set
        /// The model's desired tab state — which tabs are closed and which is active. The
        /// dock is reconciled to match this in one pass (see reconcileTabs); at build time
        /// DesiredActive also seeds the initially-active tab.
        member val ClosedTabs: Set<int> = Set.empty with get, set
        member val DesiredActive = 0 with get, set

    let private panes = ConditionalWeakTable<DockCtl, Panes>()
    let getPanes(dc: DockCtl) = panes.GetValue(dc, fun _ -> Panes())

    /// Builds the IDE layout structure (Documents carry Ids, not controls).
    type AstDockFactory(names: string[], initialTab: int) =
        inherit Factory()

        member _.Doc (id: string) (title: string) : IDockable =
            Document(Id = id, Title = title, CanClose = false, CanPin = false) :> IDockable

        member this.Solo (id: string) (title: string) : IDockable =
            let dock = DocumentDock(Id = id + "-dock", CanCreateDocument = false)
            let doc = this.Doc id title
            dock.VisibleDockables <- this.CreateList<IDockable>(doc)
            dock.ActiveDockable <- doc
            dock :> IDockable

        /// Several documents as tabs sharing one dock (the first is active).
        member this.Tabs(panes: (string * string) list) : IDockable =
            let id1 = fst panes.Head
            let dock = DocumentDock(Id = id1 + "-dock", CanCreateDocument = false)
            let docs = panes |> List.map(fun (id, title) -> this.Doc id title)
            dock.VisibleDockables <- this.CreateList<IDockable>(docs |> List.toArray)
            dock.ActiveDockable <- docs.Head
            dock :> IDockable

        /// The DSL tab dock and its documents, kept so closed tabs can be re-inserted later.
        member val DslDock: DocumentDock = null with get, set
        member val DslDocs: Document[] = [||] with get, set

        override this.CreateLayout() =
            let dslDock = DocumentDock(Id = "dsl-dock", CanCreateDocument = false)

            // DSL tabs are closeable (the Generated F#/Output panes are not); a closed tab
            // keeps its edits and can be reopened from the toolbar.
            let dslDocs =
                names
                |> Array.mapi(fun i title -> Document(Id = Ids.dsl i, Title = title, CanClose = true, CanPin = false))

            dslDock.VisibleDockables <- this.CreateList<IDockable>(dslDocs |> Array.map(fun d -> d :> IDockable))
            dslDock.ActiveDockable <- dslDocs[max 0 (min initialTab (dslDocs.Length - 1))]
            this.DslDock <- dslDock
            this.DslDocs <- dslDocs

            let editors =
                ProportionalDock(Id = "editors", Orientation = Orientation.Horizontal, Proportion = 0.68)

            editors.VisibleDockables <-
                this.CreateList<IDockable>(
                    dslDock :> IDockable,
                    this.CreateProportionalDockSplitter(),
                    this.Tabs
                        [ Ids.generated, "Generated F#"
                          Ids.syntaxTree, "Syntax Tree"
                          Ids.rewrite, "Rewrite" ]
                )

            let outputPane = this.Solo Ids.output "Output"
            outputPane.Proportion <- 0.32

            let main = ProportionalDock(Id = "main", Orientation = Orientation.Vertical)

            main.VisibleDockables <-
                this.CreateList<IDockable>(editors :> IDockable, this.CreateProportionalDockSplitter(), outputPane)

            let root = this.CreateRootDock()
            root.VisibleDockables <- this.CreateList<IDockable>(main :> IDockable)
            root.DefaultDockable <- main
            root

    /// Detach a registered pane control from whatever presenter still holds it. The registry
    /// hands out *live* control instances, and a control with two visual parents is a hard
    /// Avalonia crash — e.g. when a closed tab's presenter is torn down lazily and the
    /// document is later re-inserted, or when Dock re-binds content during layout cleanup.
    let private detachFromParent(c: Control) =
        match c.GetVisualParent() with
        | null -> ()
        | :? ContentPresenter as cp -> cp.Content <- null
        | :? ContentControl as cc -> cc.Content <- null
        | :? Panel as panel -> panel.Children.Remove c |> ignore
        | _ -> ()

    /// Once every pane control is registered, build the layout. The tab count comes from
    /// the TabNames attribute (applied before the content slots), so adding a sample to
    /// App.examples is all it takes to grow the layout.
    let tryBuild(dc: DockCtl) =
        let p = getPanes dc

        let required =
            if isNull p.Names then
                []
            else
                [ for i in 0 .. p.Names.Length - 1 -> Ids.dsl i ]
                @ [ Ids.generated; Ids.syntaxTree; Ids.rewrite; Ids.output ]

        if
            not p.Built
            && not(List.isEmpty required)
            && required |> List.forall p.Controls.ContainsKey
        then
            p.Built <- true

            // Resolve each Document's content by its Id from the live registry.
            dc.DataTemplates.Add(
                FuncDataTemplate<Document>(
                    (fun d _ ->
                        match p.Controls.TryGetValue d.Id with
                        | true, c ->
                            detachFromParent c
                            c
                        | _ -> null),
                    false
                )
            )

            let factory = AstDockFactory(p.Names, p.DesiredActive)
            let layout = factory.CreateLayout()
            factory.InitLayout(layout)
            dc.Factory <- factory
            dc.Layout <- layout
            p.Factory <- Some(factory :> Factory)
            p.DslDock <- factory.DslDock
            p.DslDocs <- factory.DslDocs

            // Report tab-header switches back to MVU (skip the initial activation).
            factory.ActiveDockableChanged.Add(fun (e: ActiveDockableChangedEventArgs) ->
                match e.Dockable with
                | :? Document as doc ->
                    match Ids.tryDslIndex doc.Id, p.ActivateFn with
                    | Some index, Some dispatch -> dispatch index
                    | _ -> ()
                | _ -> ())

            // Keep at least one DSL tab open: closing the last one empties the document dock
            // and Dock's collapse/cleanup re-binds live content mid-teardown, which crashes
            // (a control can't have two visual parents). It's also better UX for the sample.
            factory.DockableClosing.Add(fun (e: DockableClosingEventArgs) ->
                match e.Dockable with
                | :? Document as doc when (Ids.tryDslIndex doc.Id).IsSome ->
                    if factory.DslDock.VisibleDockables.Count <= 1 then
                        e.Cancel <- true
                | _ -> ())

            // Report tab closes (the X on a tab header) back to MVU.
            factory.DockableClosed.Add(fun (e: DockableClosedEventArgs) ->
                match e.Dockable with
                | :? Document as doc ->
                    match Ids.tryDslIndex doc.Id, p.CloseFn with
                    | Some index, Some dispatch -> dispatch index
                    | _ -> ()
                | _ -> ())

    let private isOpen (p: Panes) (doc: Document) =
        p.DslDock.VisibleDockables |> Seq.exists(fun d -> obj.ReferenceEquals(d, doc))

    /// Reconcile the live dock to the model's desired tab state in one pass: first close /
    /// re-insert documents so the visible set matches ClosedTabs, then activate DesiredActive.
    /// Doing both here (rather than in two racing channels) means there's a single owner of
    /// the dock's arrangement, and activation always runs after the set is correct — so the
    /// active tab can't be clobbered by the last-reopened one.
    let reconcileTabs(dc: DockCtl) =
        let p = getPanes dc

        match p.Factory with
        | Some factory when not(isNull p.DslDock) ->
            // 1. Match the visible set to ClosedTabs (no activation here).
            p.DslDocs
            |> Array.iteri(fun i doc ->
                match p.ClosedTabs.Contains i, isOpen p doc with
                | true, true -> factory.CloseDockable doc
                | false, false ->
                    let position =
                        p.DslDock.VisibleDockables
                        |> Seq.filter(fun d ->
                            match Ids.tryDslIndex d.Id with
                            | Some j -> j < i
                            | None -> false)
                        |> Seq.length

                    factory.InsertDockable(p.DslDock, doc, position)
                | _ -> ())

            // 2. Activate the desired tab, now that the visible set is correct.
            if p.DesiredActive >= 0 && p.DesiredActive < p.DslDocs.Length then
                let doc = p.DslDocs[p.DesiredActive]

                if isOpen p doc then
                    factory.SetActiveDockable doc
        | _ -> ()

module DockControl =
    let WidgetKey = Widgets.register<DockCtl>()

    let TabNames =
        Attributes.defineSimpleScalarWithEquality<string[]> "DockControl_TabNames" (fun _ newValueOpt node ->
            match newValueOpt with
            | ValueSome names ->
                let dc = node.Target :?> DockCtl
                (getPanes dc).Names <- names
                // Widget attributes (the content slots) apply before scalars, so by now the
                // controls are registered but the build still needs the other scalars (e.g.
                // TabsState's DesiredActive) — defer one dispatcher tick so the whole render
                // pass has applied, then build. tryBuild is idempotent.
                Avalonia.Threading.Dispatcher.UIThread.Post(fun () -> tryBuild dc)
            | _ -> ())

    /// The model's complete tab state: the closed set and the active index. The dock is
    /// reconciled to match in a single pass (reconcileTabs), so one channel owns the dock's
    /// arrangement — no racing closed/active updates. Before the build, the value is just
    /// stashed (DesiredActive seeds the build-time active tab); after, it reconciles live.
    /// Structural equality dedupes, so this only fires when the closed set or active changes.
    /// SetActiveDockable on the already-active tab raises no event, so this can't loop with
    /// the OnActiveTab report.
    let TabsState =
        Attributes.defineSimpleScalarWithEquality<Set<int> * int> "DockControl_TabsState" (fun _ newValueOpt node ->
            let dc = node.Target :?> DockCtl
            let p = getPanes dc

            match newValueOpt with
            | ValueSome(closed, active) ->
                p.ClosedTabs <- closed
                p.DesiredActive <- active

                if p.Built then
                    // Reconcile outside the render pass — inserting/closing/activating raises
                    // Dock events that dispatch back into MVU.
                    Avalonia.Threading.Dispatcher.UIThread.Post(fun () -> reconcileTabs dc)
            | ValueNone -> ())

    /// Raises the DSL tab index whenever Dock activates a different tab (e.g. a header click).
    let OnActiveTab: SimpleScalarAttributeDefinition<int -> MsgValue> =
        let name = "DockControl_OnActiveTab"

        let key =
            SimpleScalarAttributeDefinition.CreateAttributeData(
                ScalarAttributeComparers.noCompare,
                (fun _ (newValueOpt: (int -> MsgValue) voption) (node: IViewNode) ->
                    let panes = getPanes(node.Target :?> DockCtl)

                    match newValueOpt with
                    | ValueNone -> panes.ActivateFn <- None
                    | ValueSome fn ->
                        panes.ActivateFn <-
                            Some(fun index ->
                                let (MsgValue r) = fn index
                                Dispatcher.dispatch node r))
            )
            |> AttributeDefinitionStore.registerScalar

        { Key = key
          Name = name }

    /// Raises the DSL tab index when the user closes a tab (the X on its header).
    let OnTabClosed: SimpleScalarAttributeDefinition<int -> MsgValue> =
        let name = "DockControl_OnTabClosed"

        let key =
            SimpleScalarAttributeDefinition.CreateAttributeData(
                ScalarAttributeComparers.noCompare,
                (fun _ (newValueOpt: (int -> MsgValue) voption) (node: IViewNode) ->
                    let panes = getPanes(node.Target :?> DockCtl)

                    match newValueOpt with
                    | ValueNone -> panes.CloseFn <- None
                    | ValueSome fn ->
                        panes.CloseFn <-
                            Some(fun index ->
                                let (MsgValue r) = fn index
                                Dispatcher.dispatch node r))
            )
            |> AttributeDefinitionStore.registerScalar

        { Key = key
          Name = name }

    let private contentSlot id name =
        Attributes.definePropertyWidget<Control>
            name
            (fun target ->
                match getPanes(target :?> DockCtl).Controls.TryGetValue id with
                | true, c -> box c
                | _ -> null)
            (fun target control ->
                ensureDockStyles()
                let dc = target :?> DockCtl
                (getPanes dc).Controls[ id ] <- control
                tryBuild dc)

    /// Content slots for up to 8 DSL tabs. Attribute definitions must be registered
    /// statically, so the ceiling is fixed; the layout itself sizes to TabNames.
    let TabSlots =
        [| for i in 0..7 -> contentSlot (Ids.dsl i) $"DockControl_Tab%d{i}" |]

    let GeneratedContent = contentSlot Ids.generated "DockControl_Generated"
    let SyntaxTreeContent = contentSlot Ids.syntaxTree "DockControl_SyntaxTree"
    let RewriteContent = contentSlot Ids.rewrite "DockControl_Rewrite"
    let OutputContent = contentSlot Ids.output "DockControl_Output"

[<AutoOpen>]
module DockControlBuilders =
    type Fabulous.Avalonia.View with

        /// Creates the IDE layout: one named DSL editor tab per entry, plus the generated-F#,
        /// syntax-tree and rewrite panes (tabs in the right column) and the output console
        /// below — all live Fabulous controls hosted as dockable documents.
        static member DockControl
            (
                tabs: (string * WidgetBuilder<'msg, IFabTextEditor>)[],
                generated: WidgetBuilder<'msg, #IFabControl>,
                syntaxTree: WidgetBuilder<'msg, #IFabControl>,
                rewrite: WidgetBuilder<'msg, #IFabControl>,
                output: WidgetBuilder<'msg, #IFabControl>
            ) =
            if tabs.Length > DockControl.TabSlots.Length then
                failwith $"DockControl supports at most %d{DockControl.TabSlots.Length} tabs (got %d{tabs.Length})"

            let withTabs =
                tabs
                |> Array.indexed
                |> Array.fold
                    (fun (wb: WidgetBuilder<'msg, IFabDockControl>) (i, (_, pane)) ->
                        wb.AddWidget(DockControl.TabSlots[i].WithValue(pane.Compile())))
                    (WidgetBuilder<'msg, IFabDockControl>(
                        DockControl.WidgetKey,
                        DockControl.TabNames.WithValue(tabs |> Array.map fst)
                    ))

            withTabs
                .AddWidget(DockControl.GeneratedContent.WithValue(generated.Compile()))
                .AddWidget(DockControl.SyntaxTreeContent.WithValue(syntaxTree.Compile()))
                .AddWidget(DockControl.RewriteContent.WithValue(rewrite.Compile()))
                .AddWidget(DockControl.OutputContent.WithValue(output.Compile()))

type DockControlModifiers =
    /// Raised with the DSL tab index when Dock activates a different tab (header click, etc.).
    [<Extension>]
    static member inline onActiveTabChanged(this: WidgetBuilder<'msg, #IFabDockControl>, fn: int -> 'msg) =
        this.AddScalar(DockControl.OnActiveTab.WithValue(fn >> box >> MsgValue))

    /// Raised with the DSL tab index when the user closes that tab (the X on its header).
    [<Extension>]
    static member inline onTabClosed(this: WidgetBuilder<'msg, #IFabDockControl>, fn: int -> 'msg) =
        this.AddScalar(DockControl.OnTabClosed.WithValue(fn >> box >> MsgValue))

    /// The model's desired tab state — (closed set, active index). The dock reconciles to it
    /// in one pass: build-time it seeds the active tab; post-build it closes/reopens and
    /// activates to match (covers session restore, the explorer, and the reopen buttons).
    [<Extension>]
    static member inline tabsState(this: WidgetBuilder<'msg, #IFabDockControl>, closed: Set<int>, active: int) =
        this.AddScalar(DockControl.TabsState.WithValue((closed, active)))
