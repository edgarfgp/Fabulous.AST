namespace AstEditor

open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Templates
open Dock.Model.Core
open Dock.Model.Mvvm
open Dock.Model.Mvvm.Controls
open Dock.Avalonia.Controls
open Fabulous
open Fabulous.Avalonia

// Dock's DockControl. Aliased to avoid clashing with the `DockControl` module below.
type private DockCtl = Dock.Avalonia.Controls.DockControl

/// A Fabulous.Avalonia binding for Dock's DockControl. It hosts the IDE layout: a row of DSL
/// editor *tabs* (one dockable Document per sample) beside the generated-F# pane, with the
/// output console docked below — all live Fabulous-materialized controls.
///
/// Dock is an MVVM layout framework: the control owns a mutable tree of `IDockable` view
/// models it rearranges at runtime. We build that tree once and put each pane's
/// Fabulous-materialized Control into a Document's content (via a DataTemplate). Because
/// Fabulous reuses the child node across renders, the panes stay reactive.
type IFabDockControl =
    inherit IFabControl

[<AutoOpen>]
module private DockInterop =

    // Dock ships its own control theme; without it the DockControl renders blank. Add it to
    // the running app once, lazily, when the first DockControl materializes.
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

    /// The controls a DockControl hosts: the three DSL editor tabs, the generated and output
    /// panes, the tab titles, and whether the layout is built yet.
    type Panes() =
        member val Tab1: Control = null with get, set
        member val Tab2: Control = null with get, set
        member val Tab3: Control = null with get, set
        member val Generated: Control = null with get, set
        member val Output: Control = null with get, set
        member val Names: string[] = null with get, set
        member val Built = false with get, set

    let private panes = ConditionalWeakTable<DockCtl, Panes>()
    let getPanes (dc: DockCtl) = panes.GetValue(dc, fun _ -> Panes())

    /// A Document that carries the Fabulous-materialized Control directly (the MVVM Document
    /// has no content slot), rendered by the DataTemplate registered in `tryBuild`.
    type HostDocument(host: Control) =
        inherit Document()
        member _.Host = host

    /// Builds the IDE layout: DSL tabs | generated on top, output console below.
    type private AstDockFactory(tabs: (string * Control)[], generated: Control, output: Control) =
        inherit Factory()

        member _.HostDoc (title: string) (control: Control) : IDockable =
            HostDocument(control, Title = title, CanClose = false, CanPin = false) :> IDockable

        /// One pane in its own single-document DocumentDock.
        member this.SoloPane (title: string) (control: Control) : IDockable =
            let dock = DocumentDock(CanCreateDocument = false)
            let doc = this.HostDoc title control
            dock.VisibleDockables <- this.CreateList<IDockable>(doc)
            dock.ActiveDockable <- doc
            dock :> IDockable

        override this.CreateLayout() =
            // The DSL editors as tabs in a single DocumentDock.
            let dslDock = DocumentDock(CanCreateDocument = false)

            let dslDocs =
                tabs |> Array.map(fun (title, control) -> this.HostDoc title control)

            dslDock.VisibleDockables <- this.CreateList<IDockable>(dslDocs)
            dslDock.ActiveDockable <- dslDocs.[0]

            let editors = ProportionalDock(Orientation = Orientation.Horizontal, Proportion = 0.68)

            editors.VisibleDockables <-
                this.CreateList<IDockable>(
                    dslDock :> IDockable,
                    this.CreateProportionalDockSplitter(),
                    this.SoloPane "Generated F#" generated
                )

            let outputPane = this.SoloPane "Output" output
            outputPane.Proportion <- 0.32

            let main = ProportionalDock(Orientation = Orientation.Vertical)

            main.VisibleDockables <-
                this.CreateList<IDockable>(editors :> IDockable, this.CreateProportionalDockSplitter(), outputPane)

            let root = this.CreateRootDock()
            root.VisibleDockables <- this.CreateList<IDockable>(main :> IDockable)
            root.DefaultDockable <- main
            root

    /// Once every pane is materialized, build the layout and hand it to the control.
    let tryBuild (dc: DockCtl) =
        let p = getPanes dc

        let ready =
            [ p.Tab1; p.Tab2; p.Tab3; p.Generated; p.Output ] |> List.forall(isNull >> not)

        if not p.Built && ready then
            p.Built <- true
            dc.DataTemplates.Add(FuncDataTemplate<HostDocument>((fun d _ -> d.Host), false))

            let name i =
                if not(isNull p.Names) && i < p.Names.Length then p.Names.[i] else $"Tab {i + 1}"

            let tabs = [| name 0, p.Tab1; name 1, p.Tab2; name 2, p.Tab3 |]
            let factory = AstDockFactory(tabs, p.Generated, p.Output)
            let layout = factory.CreateLayout()
            factory.InitLayout(layout)
            dc.Factory <- factory
            dc.Layout <- layout

module DockControl =
    let WidgetKey = Widgets.register<DockCtl>()

    let TabNames =
        Attributes.defineSimpleScalarWithEquality<string[]> "DockControl_TabNames" (fun _ newValueOpt node ->
            match newValueOpt with
            | ValueSome names -> (getPanes(node.Target :?> DockCtl)).Names <- names
            | _ -> ())

    let private contentSlot name (store: Panes -> Control -> unit) (read: Panes -> Control) =
        Attributes.definePropertyWidget<Control>
            name
            (fun target -> read(getPanes(target :?> DockCtl)) |> box)
            (fun target control ->
                ensureDockStyles()
                let dc = target :?> DockCtl
                store (getPanes dc) control
                tryBuild dc)

    let Tab1 = contentSlot "DockControl_Tab1" (fun p c -> p.Tab1 <- c) (fun p -> p.Tab1)
    let Tab2 = contentSlot "DockControl_Tab2" (fun p c -> p.Tab2 <- c) (fun p -> p.Tab2)
    let Tab3 = contentSlot "DockControl_Tab3" (fun p c -> p.Tab3 <- c) (fun p -> p.Tab3)

    let GeneratedContent =
        contentSlot "DockControl_Generated" (fun p c -> p.Generated <- c) (fun p -> p.Generated)

    let OutputContent =
        contentSlot "DockControl_Output" (fun p c -> p.Output <- c) (fun p -> p.Output)

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
            let struct (n1, w1) = struct (fst tab1, (snd tab1).Compile())
            let struct (n2, w2) = struct (fst tab2, (snd tab2).Compile())
            let struct (n3, w3) = struct (fst tab3, (snd tab3).Compile())

            WidgetBuilder<'msg, IFabDockControl>(DockControl.WidgetKey, DockControl.TabNames.WithValue [| n1; n2; n3 |])
                .AddWidget(DockControl.Tab1.WithValue(w1))
                .AddWidget(DockControl.Tab2.WithValue(w2))
                .AddWidget(DockControl.Tab3.WithValue(w3))
                .AddWidget(DockControl.GeneratedContent.WithValue(generated.Compile()))
                .AddWidget(DockControl.OutputContent.WithValue(output.Compile()))
