# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `toRecursive()` on `Function` and `Value` generates `let rec`
- `UseBangExpr` generates a `use!` binding in computation expressions
- `Oak().hashDirective(...)` / `hashDirectives(...)` accept `NoWarn` and `HashDirective`, placing the directives at the top of the file (trivia such as an `#if` guard is kept). The existing overloads only accepted raw `ParsedHashDirectiveNode`s, which no builder produced
- `returnAttribute(...)` / `returnAttributes(...)` on `ExternBinding` add attributes to the return type (`extern [<MarshalAs(UnmanagedType.Bool)>] bool ...`)
- `MatchClauseExpr` overloads taking a pattern widget with a string body (and string or expression guard)
- `toPrivateRepresentation()`, `toInternalRepresentation()` and `toPublicRepresentation()` set the accessibility of a record's fields or a union's cases (`type R = private { ... }`)
- `ObjExpr` accepts `InterfaceWith(...)` blocks after its members, for object expressions that implement extra interfaces (`{ new A with member ... interface IB with member ... }`). Its `.members()` modifier, which no item could be yielded into, now takes the same items as the body

### Changed
- **Breaking:** `toPrivate()`, `toInternal()` and `toPublic()` on `Record` and `Union` now set the accessibility of the type (`type private R = { ... }`), as they do on every other type definition. They used to set the accessibility of the fields or cases, which is now `toPrivateRepresentation()` and friends

### Fixed
- The NuGet packages declared the `Apache-2.0` license while `LICENSE` and the README have always been MIT; Fabulous.AST, Fabulous.AST.Json and Fabulous.AST.Build now declare `MIT`
- Building trees on several threads at once could fail on first use with `Could not find widget collection attribute Decls on widget { Key = 0 ... }`. The library was compiled with `--realsig+`, which gives each module its own static constructor; modules sharing a file (such as `Constant`, `ConstantUnit` and `ConstantMeasure`) then deadlocked on first use, and the runtime let one thread read widget keys that were not set yet. The packages are now built without `--realsig+`
- `StructAnonRecord` printed `struct [ X: float |}`; it now prints `struct {| X: float |}`
- `OptionalValPat("name")` dropped the `?`; it now prints `?name`
- Names were wrapped in backticks where F# does not allow them: type variables in `Var("'T")`, operators in `NamedParenStarIdentPat("*")`, the `const` keyword in `StaticConstantExpr`, and active pattern names such as `Function("(|Even|Odd|)", ...)`. A qualified `Augmentation("System.String")` now prints `type System.String with` instead of ``type ``System.String`` with``
- `PrefixList` printed `type <'a, 'b> Pair`; it now prints `type ('a, 'b) Pair`
- `TyparDecl` with several constraints joined them with commas (`<'T #A , #B>`); it now prints `<'T & #A & #B>`
- Curried parameters that aren't atomic are now parenthesized in `Function`, `Member`, `Default`, `LambdaExpr`, `ParenLambdaExpr`, `AppWithLambdaExpr` and property `Getter`/`Setter` parameters: typed (`a: int`) and attributed parameters, tuples, union-case patterns with arguments (`Some x`), and `as`, `|`, `&`, `::` and `:?` patterns. Previously `Function("f", [ ParameterPat("a", String()); ParameterPat("b", Int()) ], ...)` produced the invalid `let f a: string b: int = ...`, a single typed parameter silently became the return type, and `LongIdentPat("Some", NamedPat "x")` became two parameters (`let f Some x`)
- `DoExpr`, `SingleExpr` and `SingleNode` put a space after a keyword lead (`do`, `return`, `yield!`, `lazy`, ...) by default instead of producing `doprintfn ...` / `returnx`; symbol leads such as `&&` stay attached. `.addSpace(...)` still overrides the default. Code that worked around this with a leading space (`AppExpr(" printfn", ...)`) now gets two spaces and should drop it
- A `BlockComment` passed to `triviaBefore` on a declaration (value, function, extern, `val`, member, type or module declaration) printed on the same line as the declaration, which does not parse for any declaration but the first (`let a = 1` then `(*c*) let b = 2`). It is now followed by a line break, also for a `DoExpr` yielded into a class, object expression or module; before expressions, patterns and types it still stays inline. Other expressions yielded directly into a module keep their trivia as given, so pass `newlineAfter = true` there
- `InheritOther` printed `inherit Base 1`, which does not parse; an argument that isn't already parenthesized or unit is now wrapped in parentheses
- XML docs: `ExternBinding`'s `toPublic`/`toInternal` described themselves as setting `private`; `DefaultsTo` now notes that default constraints only compile inside FSharp.Core
- `StructTuple` types printed `struct (int , string)`, which does not compile; they now print `struct (int * string)`
- `toStatic()` was ignored on `Value`, `Function` and `ValField`; class `let` bindings now print `static let` (and `static let rec`) and fields `static val`
- `toInternal()` and the other accessibility modifiers were ignored on `Abbrev`, `Enum` and `Measure`, and `typeParams(...)` on `Delegate`; they now print `type internal Abbr = int`, `type internal E = ...`, `[<Measure>] type internal m` and `type D<'T> = delegate of ...`
- `InterfaceWith("System.IDisposable")` wrapped the qualified name in backticks; it now prints `interface System.IDisposable with`
- `Rewrite.expr` and `Rewrite.typeDefn` dropped the comments, blank lines and directives attached to every node they rebuilt (any node with a changed descendant), so a rename could silently delete a `// comment` above a `let`. Rebuilt nodes now keep their trivia

### Deprecated
- `QuoteExprPat`: the F# compiler rejects every quotation pattern (FS0731), so its output never compiles. Match quotations with the active patterns in `FSharp.Quotations.Patterns` instead

## [2.0.0-pre09] - 2026-11-18

### Changed
- **Breaking:** Fabulous.AST targets `net10.0` and `netstandard2.1`; `net8.0` is dropped (.NET 8 left support on 2026-11-10), and `netstandard2.1` still serves apps on it. Fabulous.AST.Json and Fabulous.AST.Build target `net10.0`, so the Fabulous.AST.Build MSBuild task needs the .NET 10 SDK or later
- Built with the .NET 11 SDK (F# 11); tests run on `net10.0` and `net11.0`
- **Breaking:** built on **Fantomas.Core 8.1.1** (was 7.0.1). The widget DSL is unchanged, but code that builds or matches Fantomas Oak nodes directly (`EscapeHatch`, `Rewrite.expr` / `Rewrite.typeDefn` functions, `Gen.mkOak` results) must follow the Fantomas 8 Oak — see the [Fantomas upgrade guide](https://fsprojects.github.io/fantomas/docs/end-users/UpgradeGuide.html). The changes most likely to matter:
  - Chains are `ExprChain(head, segments, terminal)`. `Expr.DotLambda`, `Expr.DotIndexedGet`, `Expr.AppLongIdentAndSingleParenArg` and `Expr.NestedIndexWithoutDot` are gone; the widgets of the same name now produce a chain, `Expr.AppSingleParenArg` and an `Expr.App` of `Expr.IndexWithoutDot` respectively
  - `let`, `use`, `let!`, `use!` and `and!` inside computation expressions are all `ComputationExpressionStatement.BindingStatement` (a `BindingNode` whose leading keyword says which)
  - `TypeNameNode`'s identifier is a `Type`, and the `with` keyword moved from `TypeNameNode` to each `TypeDefn*Node`; the `ITypeDefn` interface is gone (use the node's `TypeName` / `Members`)
  - Record fields are wrapped in `ExprRecordFieldOrSpread` / `TypeDefnRecordFieldOrSpread` (F# 11 record spreads)
  - `PatRecordField` and `NamePatPair` are both `NamePatPairNode`; `ElseIfNode` is `IfKeywordNode.ElseIf(else, if)`; a match clause's `when` guard is `(whenKeyword, expr)`
  - `BindingNode`, `UnionCaseNode`, `ExceptionDefnNode` and `ExternBindingNode` take the new keyword tokens (`in`, `of`, `exception`, `static`); a union case prints `of` only when its node carries it
  - `Range.Zero` is deprecated in favour of `Range.range0`
- **Breaking:** `ChainLink` (the item type of `ChainExpr`) is now a Fabulous.AST type rather than a Fantomas one; the `ChainLink*` builders are unchanged. `ChainExpr` now throws `ArgumentException` for links that don't start with a receiver or don't alternate `ChainLinkDot()` and a step, instead of emitting code such as a lone `.`
- `Gen.run(oak)` formats with the new `Gen.DefaultConfig`: Fantomas's defaults, except that multiline lists, arrays and records keep the `Cramped` bracket style Fabulous.AST has always produced (Fantomas 8's own default is `Aligned`). `Gen.run(oak, config)` uses exactly the config you pass
- Generated whitespace follows Fantomas 8: an explicit `Newline()` trivia yields one blank line (Fantomas 7 doubled it), a comment before a type annotation is indented one level, and `NestedIndexWithoutDotExpr` renders `a[i] b`

### Added
- `Rewrite.expr` / `Rewrite.typeDefn` walk the nodes Fantomas 8 added: record spreads (`{ ...x; y = 1 }`, `{ ...T; Y: int }`) and dynamic `?` chains

### Fixed
- `WhenTyparIsStruct` renders its `struct` constraint, and several static-optimization constraints are joined with `and` (both were dropped by Fantomas 7)
- The `when` guard of a `MatchClauseExpr` is built with its keyword token, as Fantomas 8 requires

## [2.0.0-pre08] - 2026-06-01

### Added
- `Gen.parse` round-trips a widget (or a source string) through Fantomas's F# parser and returns the rendered source when it is syntactically valid, or the parser diagnostics (one per line) otherwise — for catching widget combinations that produce invalid F# (#191)
- `Rewrite.expr` / `Rewrite.exprInOak` and `Rewrite.typeDefn` / `Rewrite.typeDefnInOak`: curried, pipeline-friendly bottom-up rewriters that apply a transform to every expression (or type definition) reachable from an Oak — e.g. rename a call site everywhere, or convert a single-case union to a record (#189)
- `producesValid` test helper: like `produces`, but also re-parses the rendered output to assert it is syntactically valid F# (#191, #192)

### Changed
- **Breaking:** `Gen` is now a sealed static type instead of a module so that `run` can be overloaded; `Gen.runWith config oak` is replaced by the `Gen.run(oak, config)` overload. `Gen.mkOak` and `Gen.run oak` are unchanged (#191)
- **Breaking:** record field accessibility (`Field(...).toPrivate()` / `toInternal()` / `toPublic()`) now lifts to the whole record representation (`type R = private { ... }`) instead of emitting an invalid per-field modifier; the most restrictive accessibility wins when fields disagree (#193)

### Fixed
- Abstract member accessors no longer emit accessibility modifiers, which F# forbids on abstract slots (`abstract Y: int with get, set` rather than the invalid `with public get, public set`) (#194)

### Removed
- **Breaking:** `getterAccessibility` / `setterAccessibility` parameters on `AbstractMember` — they rendered invalid F#, since abstract slots always have the enclosing type's visibility (#194)

## [2.0.0-pre07] - 2026-05-30

### Added
- Target `net10.0` alongside `net8.0` and `netstandard2.1` for the main library; tests now multi-target `net8.0;net10.0` (#178)
- `SigMember` builder now accepts `getterAccessibility` / `setterAccessibility` parameters, matching the accessibility support already exposed on `AbstractMember` and `AutoProperty` (#178)
- Builder-time validation: `AbstractMember(..., (string * _) seq, ...)` overloads now throw `ArgumentException` when any parameter name is empty, instead of failing at render time with a `failwith` (#178)
- `toOverride()` modifier on `WidgetBuilder<BindingNode>` and `WidgetBuilder<MemberDefn>` so generated members can render F#'s `override` keyword. Applies to `Method`, `Property`, `AutoProperty`, and `PropertyGetSet` widgets (#183)
- 21 specialized tests under `src/Fabulous.AST.Tests/Specialized/` covering composition scenarios, F# 8/9/10 features (IWSAMs, anonymous struct records, units of measure, type extensions via `Augmentation`), real-world fixtures, and FsCheck property tests (#181)

### Changed
- Bump `FSharp.Core` from `8.0.403` to `10.1.300` (#178)
- Internal: extract a shared `MultipleTextsNode.CreateGetSet` helper for property-accessor rendering, used by `AbstractSlot`, `AutoProperty`, and `SigMember` (#178)
- Internal: `AbstractSlot` materializes parameter sequences before mapping to eliminate O(n²) `Seq.length` lookups and lazy-seq re-evaluation (#178)
- Internal: replace ~46 sites of `ValueOption.map Some |> defaultValue None` with the canonical `ValueOption.toOption` across the widget tree, now that FSharp.Core 10 exposes it natively (#180)
- Internal: materialize ~28 lazy `Seq.map Gen.mkOak` sequences with `Seq.toArray` at widget-attribute boundaries, eliminating re-iteration footguns when source sequences are single-shot (#180)

### Fixed
- XML docs and `static abstract` rendering for abstract slots, by deleting the duplicate `AbstractMemberModifiers` extensions that shadowed the canonical `MemberDefnModifiers` ones (#178, follow-up to #176)
- Eliminate parallel-build race that prevented `samples/Playground` from finding `FabulousAstJsonTask` on fresh checkouts; `build.fsx` now pre-builds `Fabulous.AST.Build` before the solution build (#178)
- `Delegate` widget: materialize parameters with `List.ofSeq` before `mapi` to eliminate O(n²) `Seq.length` re-evaluation and lazy-seq re-walk (#179)
- `ObjExpr` widget's `Yield` extension: replace `failwith` with `invalidArg` for argument-shape mismatches; the new message names the expected shape and the offending case (#179)
- `Measure` widget: rename the `Denominator` scalar key string from `"DivOp"` (a copy-paste leftover from the sibling scalar) to `"Denominator"` so debug output disambiguates the two scalars (#179)
- `.toRecursive()` modifier is no longer silently a no-op on `Abbrev`, `Enum`, `TypeDefnExplicit`, and `Measure`. These four widget keys now read `TypeDefn.IsRecursive` and emit `and` instead of `type` when set, so mutually-recursive type groups can include these kinds (#182)

### Removed
- Dead scalar definitions in `AutoProperty.IsStatic` and `PropertyGetSet` (`IsInlined`, `MultipleAttributes`, `IsStatic`, `Accessibility`) that were never read by any widget key (#178)

## [2.0.0-pre06] - 2026-01-09

### Added
- Auto-escape let-binding names with backticks when needed for Value and Use string overloads (#170)

## [2.0.0-pre05] - 2025-12-19

### Changed
- No changes

## [2.0.0-pre04] - 2025-12-15

### Changed
- No changes

## [2.0.0-pre03] - 2025-12-13

### Changed
- No changes

## [2.0.0-pre02] - 2025-12-12

### Changed
- No changes

## [2.0.0-pre01] - 2025-12-12

### Breaking Changes
- Widget builders now return unified base types instead of specific node types
- Raw SyntaxOak nodes must now be wrapped with EscapeHatch when yielding into collections
- Removed obsolete per-node modifiers

### Added
- Unified modifier extensions for widget types
- New EscapeHatch widget for wrapping raw SyntaxOak nodes
- New single-clause overloads for Match and Try expressions
- Parallel test execution enabled

### Changed
- Refactored widget systems to use shared attribute sets
- Internal widget key handlers now wrap nodes appropriately

### Fixed
- Widget key generation fixes

## [1.8.0] - 2025-12-06

### Changed
- feat: add support for trivia by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/161
- feat: yield from support by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/162

## [1.7.0] - 2025-12-03

### Added
- chore(docs): AI cooked the XML Docs by @shayanhabibi in https://github.com/edgarfgp/Fabulous.AST/pull/154
- feat: add support for attributes in parameter patterns by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/156
- feat: add modifiers for `Delegate` by @shayanhabibi in https://github.com/edgarfgp/Fabulous.AST/pull/160

### Fixed
* fix: Remove overload collision for Getter by @shayanhabibi in https://github.com/edgarfgp/Fabulous.AST/pull/158

## [1.6.0] - 2025-06-07

### Changed
- Expose the APIs using `seq` instead of `list` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/151

## [1.5.0] - 2025-05-10

### Changed
- AST widget updates by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/150

## [1.4.0] - 2025-04-04

### Added
- Snapshot testing by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/145
- Add a playground sample by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/148
- Improve docs by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/149

### Changed
- Update fantomas by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/146

## [1.3.0] - 2025-03-16

### Changed
* Return type changes by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/138

### Added
- Add AsyncPrefix and TaskPrefix by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/139
- Add PipeRightExpr and PipeLeftExpr widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/143

### Fixed
- Fix string interpolation widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/142

## [1.2.0] - 2025-03-05
### Fixed
- Avoid using `NormalizeIdentifierBackticks` when generating `IdentifierOrDot` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/137

## [1.1.0] - 2025-01-12

### Changed
- `Oak` and `Namespaces` fixes by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/128
- `MemberDefn` fixes by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/129

## [1.0.0] - 2024-12-15

### Changed
- Finish abstract members by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/124
- Improve widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/126

### Fixed
- Fix type augmentations by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/125

## [1.0.0-pre17] - 2024-10-06

### Added
- Gen.run in https://github.com/edgarfgp/Fabulous.AST/pull/119
- Support TriviaNode in https://github.com/edgarfgp/Fabulous.AST/pull/122

### Changed
- Better Yield Extensions in https://github.com/edgarfgp/Fabulous.AST/pull/117
- Use Fabulous.Builders in https://github.com/edgarfgp/Fabulous.AST/pull/118
- Update Fantomas in https://github.com/edgarfgp/Fabulous.AST/pull/120
- Correctly generate attributes on properties in https://github.com/edgarfgp/Fabulous.AST/pull/121

## [1.0.0-pre16] - 2024-07-14

### Added
- Delegate widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/112
- Allow ParsedHashDirectives to take non string arguments by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/116

### Changed
- Update target frameworks by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/114
- Correct typo by @sashang in https://github.com/edgarfgp/Fabulous.AST/pull/115

## [1.0.0-pre15] - 2024-07-01
  
### Added
- More widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/109
- Add AnyModuleDecl widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/111

### Changed
- Improve docs by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/110

## [1.0.0-pre14] - 2024-06-13

### Added
- `For` and `DotLambda` widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/106
- More `Expr` widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/107

### Changed
- Update `Fantomas.Core` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/105

## [1.0.0-pre13] - 2024-06-02

### Added
- ModuleAbbrev widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/104

### Changed
- Update Fantomas.Core by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/105

## [1.0.0-pre12] - 2024-05-20
### Changed
- Allow `Unit` as empty widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/102
- ModuleOrNamespaces by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/103

## [1.0.0-pre11] - 2024-05-16

### Changed
- TyparDecl Widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/96
- Use `voption` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/99
- Attributes clean up by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/101

### Fixed
- Abbrev (type alias) doesn't output XML Docs by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/101

## [1.0.0-pre10] - 2024-05-13

### Added
- Support `InterfaceEnd` and `StructEnd` widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/98

### Fixed
- Fix `EmptyInterfaceMember` widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/98

## [1.0.0-pre9] - 2024-05-13

### Added
- Support `EmptyInterfaceMember` widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/97

## [1.0.0-pre8] - 2024-05-12

### Changed
- Adds addition OS to build matrix by @TheAngryByrd in https://github.com/edgarfgp/Fabulous.AST/pull/94

### Fixed
- Fix AbstractSlot AnonRecord generation by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/95

## [1.0.0-pre7] - 2024-05-10

### Changed
- Update to use `netsandard2.1` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/93

## [1.0.0-pre6] - 2024-05-10

### Changed
- RecordPat widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/85
- Unify Constant widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/86
- Remove `Extension` attribute on types by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/87
- InheritRecordExpr by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/88
- Inherit Widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/89
- Member LetBinding widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/90
- ExplicitCtor Widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/91
- Normalizing Widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/92

## [1.0.0-pre5] - 2024-04-14

### Added
- WhileExpr widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/82
- AutoProperty widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/83

### Changed
- Update Property widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/84

## [1.0.0-pre4] - 2024-04-07

### Added
- Add support for `ExprForEach` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/81

## [1.0.0-pre3] - 2024-04-01

### Changed
- More Widget `Expr` by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/80

## [1.0.0-pre2] - 2024-03-27

### Changed
- Use `WidgetBuilder` for `Expr` and `Pattern` for simplicity by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/79

## [1.0.0-pre1] - 2024-03-25

### Added
- Add Union samples by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/74
- Add record samples by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/75
- Support AppLongIdentAndSingleParenArg by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/73
- Add support for multiple bindings in a method by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/78

### Changed
- Update AttributeCollectionBuilder by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/76
- Rework hasQuotes by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/71

### Fixed
- Fix missing open statement in example code by @lamg in https://github.com/edgarfgp/Fabulous.AST/pull/72

## [0.9.0] - 2024-03-12

### Fixed
-  Unable to return IfThenElse expression from a Method by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/70

## [0.8.2] - 2024-03-11

### Changed
-  Add backticks for reserved keywords by @JordanMarr in https://github.com/edgarfgp/Fabulous.AST/pull/68

## [0.8.1] - 2024-03-10

### Added
- Escape content in from String by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/64

## [0.8.0] - 2024-03-08

### Changed
- Rework constant widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/57
- Update fsdocs-tool to 20.0.0
- Unify Module and Namespace

### Added
- Attribute Widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/58
- Type widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/59
- Add ConstantStringExpr widget

## [0.7.0] - 2023-02-11

### Added
- Interface Widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/40
- Initial support for unit of measures by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/50
- Interface abstract members widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/48
- Common types by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/47

### Changed
- Update fantomas by @kaashyapan in https://github.com/edgarfgp/Fabulous.AST/pull/38
- Rework Class Widget by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/42
- Record Unions Generic Widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/45
- Widgets for TypeAugmentation member methods and patterns by @kaashyapan in https://github.com/edgarfgp/Fabulous.AST/pull/46
- Update fsharp formatting to modern theme by @nojaf in https://github.com/edgarfgp/Fabulous.AST/pull/49
- Unify Record and GenericRecord into a single node by @TimLariviere in https://github.com/edgarfgp/Fabulous.AST/pull/52
- Simplify union definitions by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/53
- Classes Unification by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/54
- Interfaces unification by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/55
- BindingNode cleanup by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/51
- DSL clean-up by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/56

## [0.6.2] - 2023-06-07
### Added
- Attributes for Unions and Emums
- Enum/Unions/Records fields attributes support
- TypeArgs for Unions and Records

## [0.6.1] - 2023-06-04
### Added
- Add initial support for Classes

### Fixed
- Fix Multiple for loops in builder causes crash

## [0.6.0] - 2023-05-29
### Added
- Add initial support for `IfThen` expression
- Add initial support for `IfThenElse` expression
- Add initial support for `IfThenElIf` expression
- Add supprt for `yield!` expression for `CollectionBuilder`

### Changed
- Update `Fantomas.Core` 6.0.3

## [0.5.0] - 2023-04-21
### Added
- Add initial support for `IfThen` expression

### Changed
- Rename `Alias` to `Abbrev` to match AST

## [0.4.0] - 2023-04-13
### Added
- Add initial support for enums and type aliases

### Changed
- Update `Fantomas.Core` 6.0.0

## [0.3.0] - 2023-04-11
### Added
- Add initial support for members in Record and Unions

### Changed
- Update `Fantomas.Core` 6.0.0-beta-001

## [0.2.0] - 2023-04-06
### Added
- Add initial support for Record TypeDeclaration. @nojaf

### Changed
- Target netstandard2.1 @nojaf
- Use fixed version of Fantomas.Core @nojaf

## [0.1.0] - 2023-04-03

- Initial release

[unreleased]: https://github.com/edgarfgp/Fabulous.AST/compare/2.0.0-pre09...HEAD
[2.0.0-pre09]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre09
[2.0.0-pre08]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre08
[2.0.0-pre07]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre07
[2.0.0-pre06]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre06
[2.0.0-pre05]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre05
[2.0.0-pre04]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre04
[2.0.0-pre03]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre03
[2.0.0-pre02]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre02
[2.0.0-pre01]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre01
[1.8.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.8.0
[1.7.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.7.0
[1.6.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.6.0
[1.5.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.5.0
[1.4.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.4.0
[1.3.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.3.0
[1.2.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.2.0
[1.1.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.1.0
[1.0.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0
[1.0.0-pre17]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre17
[1.0.0-pre16]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre16
[1.0.0-pre15]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre15
[1.0.0-pre14]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre14
[1.0.0-pre13]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre13
[1.0.0-pre12]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre12
[1.0.0-pre11]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre11
[1.0.0-pre10]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre10
[1.0.0-pre9]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre9
[1.0.0-pre8]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre8
[1.0.0-pre7]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre7
[1.0.0-pre6]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre6
[1.0.0-pre5]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre5
[1.0.0-pre4]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre4
[1.0.0-pre3]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre3
[1.0.0-pre2]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre2
[1.0.0-pre1]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/1.0.0-pre1
[0.9.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.9.0
[0.8.2]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.8.2
[0.8.1]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.8.1
[0.8.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.8.0
[0.7.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.7.0
[0.6.2]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.2
[0.6.1]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.1
[0.6.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.0
[0.5.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.5.0
[0.4.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.4.0
[0.3.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.3.0
[0.2.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.2.0
[0.1.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.1.0
