namespace AstEditor

open System.ComponentModel
open System.Runtime.CompilerServices
open Avalonia.Threading
open AvaloniaEdit
open AvaloniaEdit.CodeCompletion
open FSharp.Compiler.EditorServices

/// Parameter/overload hints for the DSL editor. When the user types `(`, we resolve the
/// method name before it, ask FCS for its overloads, and show them in AvaloniaEdit's
/// OverloadInsightWindow (↑/↓ cycles overloads).
module SignatureHelp =

    /// Drives the OverloadInsightWindow from the FCS overload list.
    type private OverloadProvider(items: Intellisense.SignatureOverload[]) =
        let ev = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
        let mutable index = 0
        // Sender is ignored by the window's bindings; only the property name matters.
        let notify name = ev.Trigger(null, PropertyChangedEventArgs(name))

        interface INotifyPropertyChanged with
            [<CLIEvent>]
            member _.PropertyChanged = ev.Publish

        interface IOverloadProvider with
            member _.Count = items.Length

            member _.SelectedIndex
                with get () = index
                and set v =
                    index <- max 0 (min v (items.Length - 1)) // clamp; never index out of range

                    for p in [ "SelectedIndex"; "CurrentIndexText"; "CurrentHeader"; "CurrentContent" ] do
                        notify p

            member _.CurrentIndexText = $"{index + 1} of {items.Length}"
            member _.CurrentHeader = box items.[index].Header

            member _.CurrentContent =
                let ps = items.[index].Parameters
                box (if ps.Length > 0 then String.concat "\n" ps else "(no parameters)")

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    let install (editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())
            let mutable current: OverloadInsightWindow = null

            editor.TextArea.TextEntered.Add(fun e ->
                if e.Text = "(" then
                    let doc = editor.Document
                    let loc = doc.GetLocation(editor.CaretOffset) // caret sits just after '('
                    let line = loc.Line
                    let lineText = doc.GetText(doc.GetLineByNumber(line))
                    let nameIndex = loc.Column - 3 // last char of the name, before '('

                    if nameIndex >= 0 && nameIndex < lineText.Length then
                        match QuickParse.GetCompleteIdentifierIsland false lineText nameIndex with
                        | Some(island, colAtEndOfNames, _) ->
                            let names = island.Split('.') |> List.ofArray
                            let source = editor.Text

                            async {
                                try
                                    let! overloads = Intellisense.signatures source line colAtEndOfNames lineText names

                                    if overloads.Length > 0 then
                                        Dispatcher.UIThread.Post(fun () ->
                                            // Close any previous hint so nested calls don't stack windows.
                                            if not(isNull current) then
                                                current.Close()

                                            let w = OverloadInsightWindow(editor.TextArea)
                                            w.Closed.Add(fun _ -> current <- null)
                                            w.Provider <- OverloadProvider(overloads)
                                            w.Show()
                                            current <- w)
                                with _ ->
                                    ()
                            }
                            |> Async.Start
                        | None -> ())
