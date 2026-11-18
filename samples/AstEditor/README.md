# AstEditor — a live Fabulous.AST playground

A small desktop editor for exploring [Fabulous.AST](../../). Write the Fabulous.AST DSL on the
left and the generated F# appears on the right as you type. Problems, run output and the
generated code's syntax tree live in the bottom panel.

Built with [Fabulous.Avalonia](https://github.com/fabulous-dev/Fabulous.Avalonia) (MVU),
[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (code editor) and
[FSharp.Compiler.Service](https://www.nuget.org/packages/FSharp.Compiler.Service) (type-checking,
IntelliSense and evaluation).

## Run

```bash
dotnet run --project samples/AstEditor
```

It targets `net10.0` and rolls forward, so a machine with only a newer runtime (e.g. .NET 11)
runs it too. The first check after launch warms up the F# compiler (a few seconds); edits are
fast afterwards.

## Using it

| | |
|---|---|
| **Edit** | Type the DSL. ~400 ms after you pause, the script is type-checked; if it's clean it is evaluated and **Generated.fs** updates. |
| **Problems** | Errors and warnings appear as squiggles and in the **Problems** panel (count in the tab badge and status bar). Click a problem to select its range. While the script has errors, the generated pane keeps the last good output, dimmed, under an "out of date" banner. |
| **IntelliSense** | Completion (incl. members after `.`), hover tooltips, and signature help on `(`. |
| **Run** | **▶ Run** (⌘↵ / Ctrl+Enter) executes the generated F# and shows its console output in **Output**. |
| **Syntax tree** | **Syntax Tree** shows the generated code's untyped AST. Click a node to select the code it covers in Generated.fs. |
| **Files** | **New** / **+** (⌘N / Ctrl+N) opens a blank script, **Examples** opens a built-in example (Hello, Record, Union, Class, Functions, Rewrite), **×** / ⌘W closes a tab. |
| **Theme** | System (follows the OS live), Light or Dark. |
| **Session** | Open tabs, their contents and the active tab are restored on the next launch. |

A script is ordinary F#: opens, helpers, a `Rewrite` pass, … ending in the generated source,
typically `… |> Gen.mkOak |> Gen.run`. `#r` / `#load` lines (e.g. from pasted docs snippets)
are ignored; the Fabulous.AST assemblies are already referenced.

## Architecture

The app is Fabulous MVU. `App.update` owns one pipeline per edit: **debounce → FCS type-check →
(if clean) FSI evaluate → parse the output's syntax tree**. Every async result is tagged with the
edit version it was started for, so stale results are dropped.

| File | Responsibility |
|------|----------------|
| `App.fs` | Model / update / view: documents, the check → generate pipeline, layout. |
| `Styles.axaml` | All colours and states (hover, active tab, …) for light and dark; the view only assigns classes. |
| `Examples.fs` | The built-in example scripts. |
| `Evaluator.fs` | Two FSI sessions: one evaluates the DSL (`generate`), one runs the generated code (`run`), so a hanging run can't block generation. Per-thread console capture. |
| `Intellisense.fs` | FCS language service: diagnostics, completion, tooltips, signatures, parsing. |
| `SyntaxTree.fs` | Hand-written walk of the untyped AST into a readable tree with a source span per line. |
| `AvaloniaEditView.fs` | Fabulous binding for AvaloniaEdit's `TextEditor` (two-way text, caret, selection requests, diagnostics, TextMate highlighting). |
| `Squiggles.fs` / `DiagnosticsStore.fs` | Draw the model's diagnostics as squiggles; share them with hover. |
| `Completion.fs` / `Hover.fs` / `SignatureHelp.fs` | Wire FCS results to AvaloniaEdit's completion, hover and overload windows. |
| `Session.fs` | Saves/restores the open documents (JSON in the user's app-data folder). |

## Troubleshooting

Set `ASTEDITOR_TRACE=1` to log every MVU message and every thrown exception to stderr.

## Notes

- This is a **sample**, not a product. Member completion needs a script that type-checks up to
  the caret.
- **Run** has no cancel: generated code that never returns keeps "Running…" until restart (it
  can't block generation, which uses its own session).
- It references the published **Fabulous.AST** package (not the project) so its FSharp.Core
  lines up with the compiler service.
