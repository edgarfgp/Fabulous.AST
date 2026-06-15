# AstEditor — a live Fabulous.AST playground

A small desktop IDE for exploring [Fabulous.AST](../../). You write the Fabulous.AST DSL on
the left and watch the generated F# source appear on the right, then **Run** it and see its
output — all in a dockable, dark-themed editor with F# syntax highlighting and IntelliSense.

Built with [Fabulous.Avalonia](https://github.com/fabulous-dev/Fabulous.Avalonia) (MVU),
[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) (code editor),
[Dock](https://github.com/wieslawsoltes/Dock) (docking layout) and
[FSharp.Compiler.Service](https://www.nuget.org/packages/FSharp.Compiler.Service) (evaluation
+ IntelliSense).

## Run

```bash
dotnet run --project samples/AstEditor
```

> The first completion / generation triggers a cold F# Interactive + compiler-service warm-up
> (~1–2s); everything is fast afterwards.

## What it does

- **Live generation** — type the DSL, and ~400ms after you pause it is evaluated in a hosted
  FSI session and the generated F# is rendered on the right.
- **Run** — executes the generated F# and shows its console output in the bottom console.
- **IntelliSense** (DSL pane) — FSharp.Compiler.Service powered:
  - type-aware completion, including member completion after `.`, with category glyphs
  - hover tooltips (and the diagnostic message when you hover a squiggle)
  - signature help (overloads) when you type `(`
  - inline error/warning squiggles
  - quick-fix *repairs* — offered while the script has errors: a 💡 **lightbulb** appears in
    the gutter (or press **Ctrl+.**); e.g. rewrite a typo'd `Fielddd` to `Field`
  - *refactorings* in the same menu, available on valid code too — "Apply constant-folding
    Rewrite" threads a `Rewrite.expr` pass into the DSL pipeline, and "Convert Record to
    Union (DU)" rewrites a `Record(…) { Field… }` block into a `Union(…) { UnionCase… }`
- **Sample tabs** — each example (Record / Rewrite / Hello / Quick Fix / Convert) is its own
  DSL editor **tab** you can drag out, float, re-dock and **close** (reopen it from the
  toolbar's ⊕ button); selecting a tab (header click, click-in, or edit) makes it the active
  tab driving the generated and output panes. The *Quick Fix* tab ships with a deliberate
  typo so the lightbulb flow is one squiggle away; the *Convert* tab walks through the
  Record→DU action.
- **Theme switcher** — a toolbar group (System / Light / Dark) that flips the app variant
  *and* re-themes the editors' TextMate syntax colours to match, live.
- **Status bar** — a live `Ln x, Col y` position pill that tracks the DSL editor's caret.
- **Session restore** — your per-tab edits and the active tab are saved on close and restored
  on the next launch.
- **Code editing** — AvaloniaEdit with TextMate F# syntax highlighting, line numbers, undo.
- **Docking** — the three panes are Dock documents you can drag, split and float.

The DSL you write is a full F# script (opens, helpers, an active pattern, a `Rewrite` pass…)
that ends in the generated source — typically `... |> Gen.mkOak |> Gen.run`. The default
sample demonstrates a constant-folding `Rewrite` over a small module.

## Architecture

The app is Fabulous MVU. The interesting work is in the bindings that bridge non-Fabulous
controls into the MVU world:

| File | Responsibility |
|------|----------------|
| `Program.fs` | Avalonia entry point. |
| `App.fs` | MVU `Model`/`Msg`/`update`/`view` — the IDE chrome (toolbar, status bar, dark theme) and the panes. |
| `Evaluator.fs` | Hosts an FSI session; `generate` (DSL → F#) and `run` (execute generated F#), serialized and `Console.Out`-captured. |
| `Intellisense.fs` | FSharp.Compiler.Service language service: `complete`, `tooltip`, `diagnostics`, `signatures`. |
| `DiagnosticsStore.fs` | Shares the latest diagnostics between the squiggle renderer and hover. |
| `Completion.fs` | Wires FCS completion (with glyphs) to AvaloniaEdit's `CompletionWindow`. |
| `Hover.fs` | Wires FCS tooltips — and squiggle messages — to AvaloniaEdit's `PointerHover`. |
| `Squiggles.fs` | `IBackgroundRenderer` drawing FCS diagnostics as wavy underlines (debounced). |
| `SignatureHelp.fs` | Wires FCS overloads to AvaloniaEdit's `OverloadInsightWindow` on `(`. |
| `QuickFix.fs` | Ctrl+. code actions: FCS "did you mean" repairs (error-driven) + the always-available refactorings. |
| `RewriteAction.fs` | The `Rewrite`-powered code action — rewrites the DSL to add a constant-folding `Rewrite.expr` pass. |
| `ConvertAction.fs` | The Record→DU code action — rewrites a `Record { Field… }` block into a `Union { UnionCase… }`. |
| `Lightbulb.fs` | A custom gutter `AbstractMargin` drawing the 💡 on actionable lines; click opens the picker. |
| `AvaloniaEditView.fs` | A hand-written Fabulous.Avalonia binding for AvaloniaEdit's `TextEditor` (two-way text, line numbers, TextMate, the IntelliSense hooks). |
| `DockView.fs` | A Fabulous.Avalonia binding for Dock's `DockControl` — hosts the DSL editor **tabs** (one Id-keyed Document per sample, content resolved by Id) plus the generated/output panes, all **live** Fabulous controls. |
| `Session.fs` | Persists per-tab edits + the active tab across restarts (a small JSON in app-data). |

### Notable bridges

- **Fabulous ⇆ AvaloniaEdit** (`AvaloniaEditView.fs`): AvaloniaEdit's `Text` is a CLR property
  (no `AvaloniaProperty`) and `TextChanged` is a parameterless event, so the two-way binding is
  hand-wired. Disposing the change handler *before* a programmatic text set keeps the caret
  stable.
- **Fabulous ⇆ Dock** (`DockView.fs`): Dock owns a mutable, build-once view-model tree, which is
  the opposite of MVU's re-render model. The binding uses Fabulous's `definePropertyWidget` to
  materialize each pane into a reactive `Control`, then hands those controls to Dock as
  `Document` content via a `DataTemplate` — so the panes stay live while Dock owns the layout.

## Notes

- This is a **sample**, not a product — it favours clarity over completeness. Member completion
  needs a successful type-check, and the FCS warm-up is visible on first use.
- **Session** (edits + active tab) is restored, but the dock **arrangement** (floating/resizing)
  is not: Dock's `SystemTextJson` serializer (11.3.12.1) doesn't round-trip a layout that holds
  live controls — it'd need the fixes that currently live only in Dock's `master`. The documents
  are already Id-keyed (the prerequisite), so this is a drop-in once a fixed serializer ships.
- It pins Avalonia **11.3.12** (Dock requires it) and references the published **Fabulous.AST**
  package so its FSharp.Core lines up with the compiler service.
