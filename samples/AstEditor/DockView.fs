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

/// A Fabulous.Avalonia binding for Dock's DockControl, hosting three live Fabulous panes
/// (source / generated / output) as dockable Documents the user can drag, split and float.
///
/// Dock is an MVVM layout framework: the control owns a mutable tree of `IDockable` view
/// models that it rearranges at runtime. We build that tree once (in the Factory) and put
/// each pane's *Fabulous-materialized Control* into a Document's Content. Because Fabulous
/// reuses the child node across renders, those panes stay fully reactive even though the
/// dock tree itself is opaque, build-once state.
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

    /// The three pane controls a DockControl hosts, plus whether the layout is built yet.
    type Panes() =
        member val Source: Control = null with get, set
        member val Generated: Control = null with get, set
        member val Output: Control = null with get, set
        member val Built = false with get, set

    let private panes = ConditionalWeakTable<DockCtl, Panes>()
    let getPanes (dc: DockCtl) = panes.GetValue(dc, fun _ -> Panes())

    /// A Document that carries the Fabulous-materialized Control directly (the MVVM Document
    /// has no Content slot), rendered by the DataTemplate registered in `tryBuild`.
    type HostDocument(host: Control) =
        inherit Document()
        member _.Host = host

    /// Builds the dock layout: three side-by-side Documents (DSL | Generated | Output),
    /// each hosting one of the Fabulous-materialized pane controls.
    type private AstDockFactory(source: Control, generated: Control, output: Control) =
        inherit Factory()

        member this.Pane (title: string) (control: Control) : IDockable =
            // CanClose/CanPin off: these three panes are essential, so don't let them be
            // closed away. Drag/float stay enabled. CanCreateDocument off hides the "+"
            // button (we have no document factory to back it).
            let doc = HostDocument(control, Title = title, CanClose = false, CanPin = false)
            let dock = DocumentDock(CanCreateDocument = false)
            dock.VisibleDockables <- this.CreateList<IDockable>(doc :> IDockable)
            dock.ActiveDockable <- doc
            dock :> IDockable

        override this.CreateLayout() =
            // IDE layout: the two code editors side-by-side on top, the output console
            // docked across the bottom.
            let editors = ProportionalDock(Orientation = Orientation.Horizontal, Proportion = 0.68)

            editors.VisibleDockables <-
                this.CreateList<IDockable>(
                    this.Pane "DSL" source,
                    this.CreateProportionalDockSplitter(),
                    this.Pane "Generated F#" generated
                )

            let outputPane = this.Pane "Output" output
            outputPane.Proportion <- 0.32

            let main = ProportionalDock(Orientation = Orientation.Vertical)

            main.VisibleDockables <-
                this.CreateList<IDockable>(editors :> IDockable, this.CreateProportionalDockSplitter(), outputPane)

            let root = this.CreateRootDock()
            root.VisibleDockables <- this.CreateList<IDockable>(main :> IDockable)
            root.DefaultDockable <- main
            root

    /// Once all three panes are materialized, build the layout and hand it to the control.
    /// Built once: this assumes each pane's root widget type is stable (so Fabulous reuses
    /// the same control across renders and never re-invokes `set`). The current panes —
    /// TextEditor, Grid, TextEditor — satisfy that.
    let tryBuild (dc: DockCtl) =
        let p = getPanes dc

        if not p.Built && not(isNull p.Source) && not(isNull p.Generated) && not(isNull p.Output) then
            p.Built <- true
            // Render each HostDocument's body as the control it carries.
            dc.DataTemplates.Add(FuncDataTemplate<HostDocument>((fun d _ -> d.Host), false))
            let factory = AstDockFactory(p.Source, p.Generated, p.Output)
            let layout = factory.CreateLayout()
            factory.InitLayout(layout)
            dc.Factory <- factory
            dc.Layout <- layout

module DockControl =
    let WidgetKey = Widgets.register<DockCtl>()

    let private contentSlot name (store: Panes -> Control -> unit) (read: Panes -> Control) =
        Attributes.definePropertyWidget<Control>
            name
            (fun target -> read(getPanes(target :?> DockCtl)) |> box)
            (fun target control ->
                ensureDockStyles()
                let dc = target :?> DockCtl
                store (getPanes dc) control
                tryBuild dc)

    let SourceContent =
        contentSlot "DockControl_Source" (fun p c -> p.Source <- c) (fun p -> p.Source)

    let GeneratedContent =
        contentSlot "DockControl_Generated" (fun p c -> p.Generated <- c) (fun p -> p.Generated)

    let OutputContent =
        contentSlot "DockControl_Output" (fun p c -> p.Output <- c) (fun p -> p.Output)

[<AutoOpen>]
module DockControlBuilders =
    type Fabulous.Avalonia.View with

        /// Creates a Dock layout hosting three live Fabulous panes as dockable documents.
        static member inline DockControl
            (
                source: WidgetBuilder<'msg, #IFabControl>,
                generated: WidgetBuilder<'msg, #IFabControl>,
                output: WidgetBuilder<'msg, #IFabControl>
            ) =
            WidgetBuilder<'msg, IFabDockControl>(
                DockControl.WidgetKey,
                DockControl.SourceContent.WithValue(source.Compile())
            )
                .AddWidget(DockControl.GeneratedContent.WithValue(generated.Compile()))
                .AddWidget(DockControl.OutputContent.WithValue(output.Compile()))
