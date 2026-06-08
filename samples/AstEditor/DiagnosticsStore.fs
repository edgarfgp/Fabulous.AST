namespace AstEditor

open System.Runtime.CompilerServices
open AvaloniaEdit

/// The latest diagnostics per editor, shared between the squiggle renderer (which writes them)
/// and hover (which reads them, to show the error message under the pointer).
module DiagnosticsStore =

    let private table = ConditionalWeakTable<TextEditor, Intellisense.Diagnostic[] ref>()

    let private slot (editor: TextEditor) =
        table.GetValue(editor, fun _ -> ref [||])

    // Notified (on the UI thread) whenever an editor's diagnostics change — the lightbulb
    // gutter uses this to redraw.
    let private listeners = ResizeArray<TextEditor -> unit>()
    let onChanged (f: TextEditor -> unit) = listeners.Add f

    let set (editor: TextEditor) (diags: Intellisense.Diagnostic[]) =
        (slot editor).Value <- diags

        for f in listeners do
            f editor

    let get (editor: TextEditor) = (slot editor).Value

    /// Is the (1-based line, 0-based column) position inside this diagnostic's range?
    let covers (line: int) (col: int) (d: Intellisense.Diagnostic) =
        let afterStart =
            line > d.StartLine || (line = d.StartLine && col >= d.StartColumn)

        let beforeEnd = line < d.EndLine || (line = d.EndLine && col <= d.EndColumn)
        afterStart && beforeEnd
