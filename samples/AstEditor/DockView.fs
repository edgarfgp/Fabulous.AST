namespace AstEditor

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Templates
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
    let output = "output"
    let tryDslIndex (id: string) =
        if not(isNull id) && id.StartsWith("dsl-", StringComparison.Ordinal) then
            match Int32.TryParse(id.Substring 4) with
            | true, i -> Some i
            | _ -> None
        else
            None

[<AutoOpen>]
module private DockInterop =

    let mutable private stylesAdded = false

    let ensureDockStyles () =
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

    let private panes = ConditionalWeakTable<DockCtl, Panes>()
    let getPanes (dc: DockCtl) = panes.GetValue(dc, fun _ -> Panes())

    /// Builds the IDE layout structure (Documents carry Ids, not controls).
    type AstDockFactory(names: string[]) =
        inherit Factory()

        member _.Doc (id: string) (title: string) : IDockable =
            Document(Id = id, Title = title, CanClose = false, CanPin = false) :> IDockable

        member this.Solo (id: string) (title: string) : IDockable =
            let dock = DocumentDock(Id = id + "-dock", CanCreateDocument = false)
            let doc = this.Doc id title
            dock.VisibleDockables <- this.CreateList<IDockable>(doc)
            dock.ActiveDockable <- doc
            dock :> IDockable

        override this.CreateLayout() =
            let dslDock = DocumentDock(Id = "dsl-dock", CanCreateDocument = false)
            let dslDocs = names |> Array.mapi(fun i title -> this.Doc (Ids.dsl i) title)
            dslDock.VisibleDockables <- this.CreateList<IDockable>(dslDocs)
            dslDock.ActiveDockable <- dslDocs.[0]

            let editors = ProportionalDock(Id = "editors", Orientation = Orientation.Horizontal, Proportion = 0.68)

            editors.VisibleDockables <-
                this.CreateList<IDockable>(
                    dslDock :> IDockable,
                    this.CreateProportionalDockSplitter(),
                    this.Solo Ids.generated "Generated F#"
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

    /// Once every pane control is registered, build the layout.
    let tryBuild (dc: DockCtl) =
        let p = getPanes dc
        let required = [ Ids.dsl 0; Ids.dsl 1; Ids.dsl 2; Ids.generated; Ids.output ]

        if not p.Built && required |> List.forall p.Controls.ContainsKey then
            p.Built <- true

            // Resolve each Document's content by its Id from the live registry.
            dc.DataTemplates.Add(
                FuncDataTemplate<Document>(
                    (fun d _ ->
                        match p.Controls.TryGetValue d.Id with
                        | true, c -> c
                        | _ -> null),
                    false
                )
            )

            let names =
                if not(isNull p.Names) && p.Names.Length >= 3 then p.Names else [| "Tab 1"; "Tab 2"; "Tab 3" |]

            let factory = AstDockFactory(names)
            let layout = factory.CreateLayout()
            factory.InitLayout(layout)
            dc.Factory <- factory
            dc.Layout <- layout

            // Report tab-header switches back to MVU (skip the initial activation).
            factory.ActiveDockableChanged.Add(fun (e: ActiveDockableChangedEventArgs) ->
                match e.Dockable with
                | :? Document as doc ->
                    match Ids.tryDslIndex doc.Id, p.ActivateFn with
                    | Some index, Some dispatch -> dispatch index
                    | _ -> ()
                | _ -> ())

module DockControl =
    let WidgetKey = Widgets.register<DockCtl>()

    let TabNames =
        Attributes.defineSimpleScalarWithEquality<string[]> "DockControl_TabNames" (fun _ newValueOpt node ->
            match newValueOpt with
            | ValueSome names -> (getPanes(node.Target :?> DockCtl)).Names <- names
            | _ -> ())

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

        { Key = key; Name = name }

    let private contentSlot id name =
        Attributes.definePropertyWidget<Control>
            name
            (fun target ->
                match (getPanes(target :?> DockCtl)).Controls.TryGetValue id with
                | true, c -> box c
                | _ -> null)
            (fun target control ->
                ensureDockStyles()
                let dc = target :?> DockCtl
                (getPanes dc).Controls.[id] <- control
                tryBuild dc)

    let Tab1 = contentSlot (Ids.dsl 0) "DockControl_Tab1"
    let Tab2 = contentSlot (Ids.dsl 1) "DockControl_Tab2"
    let Tab3 = contentSlot (Ids.dsl 2) "DockControl_Tab3"
    let GeneratedContent = contentSlot Ids.generated "DockControl_Generated"
    let OutputContent = contentSlot Ids.output "DockControl_Output"

[<AutoOpen>]
module DockControlBuilders =
    type Fabulous.Avalonia.View with

        /// Creates the IDE layout: three named DSL editor tabs, plus the generated and output
        /// panes — all live Fabulous controls hosted as dockable documents.
        static member inline DockControl
            (
                tab1: string * WidgetBuilder<'msg, #IFabControl>,
                tab2: string * WidgetBuilder<'msg, #IFabControl>,
                tab3: string * WidgetBuilder<'msg, #IFabControl>,
                generated: WidgetBuilder<'msg, #IFabControl>,
                output: WidgetBuilder<'msg, #IFabControl>
            ) =
            WidgetBuilder<'msg, IFabDockControl>(
                DockControl.WidgetKey,
                DockControl.TabNames.WithValue [| fst tab1; fst tab2; fst tab3 |]
            )
                .AddWidget(DockControl.Tab1.WithValue((snd tab1).Compile()))
                .AddWidget(DockControl.Tab2.WithValue((snd tab2).Compile()))
                .AddWidget(DockControl.Tab3.WithValue((snd tab3).Compile()))
                .AddWidget(DockControl.GeneratedContent.WithValue(generated.Compile()))
                .AddWidget(DockControl.OutputContent.WithValue(output.Compile()))

type DockControlModifiers =
    /// Raised with the DSL tab index when Dock activates a different tab (header click, etc.).
    [<Extension>]
    static member inline onActiveTabChanged(this: WidgetBuilder<'msg, #IFabDockControl>, fn: int -> 'msg) =
        this.AddScalar(DockControl.OnActiveTab.WithValue(fn >> box >> MsgValue))
