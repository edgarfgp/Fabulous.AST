# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
_No unreleased changes_

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

[unreleased]: https://github.com/edgarfgp/Fabulous.AST/compare/1.0.0-pre12...HEAD
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
