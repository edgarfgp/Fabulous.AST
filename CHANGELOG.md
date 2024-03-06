# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Updated
- Rework constant widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/57
- Update fsdocs-tool to 20.0.0
- Unify Module and Namespace

### Added
- Attribute Widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/58
- Type widgets by @edgarfgp in https://github.com/edgarfgp/Fabulous.AST/pull/59
- ConstantStringExpr

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

[unreleased]: https://github.com/edgarfgp/Fabulous.AST/compare/0.7.0...HEAD
[0.7.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.7.0
[0.6.2]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.2
[0.6.1]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.1
[0.6.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.6.0
[0.5.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.5.0
[0.4.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.4.0
[0.3.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.3.0
[0.2.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.2.0
[0.1.0]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/0.1.0
