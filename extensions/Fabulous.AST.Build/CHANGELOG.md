# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0-pre04] - 2025-12-15

### Changed
- Renamed properties for future extensibility such as OpenAPI support
- Renamed ReorderCompileItems target to FabulousAstReorderCompile
- Default file suffix changed from .cg.fs to .g.fs

### Added
- FabulousAstGeneratedFiles property as glob pattern for IDE visibility
- DependsOnFabulousAst compile item metadata for correct F# file ordering

### Fixed
- Fixed MSBuild property evaluation order
- Fixed IDE IntelliSense not recognizing generated files
- Skip FabulousAstReorderCompile target during design-time builds to prevent IDE confusion

## [2.0.0-pre03] - 2025-12-13

### Fixed
- Fix module code generation to emit proper file-level modules module A.B without backticks or equals sign
- Fix clean target to correctly delete files with custom OutputFileName

### Changed
- Simplify MSBuild targets and props configuration
- Remove redundant metadata options
- Drop namespace support only file-level modules via ModuleName are now supported

### Removed
- FabulousAstJsonOutputFileNameSuffix property hardcoded to .Generated.fs
- GeneratedFileName and OutFile item metadata use OutputFileName

## [2.0.0-pre02] - 2025-12-12

### Fixed
- Ensure `FSharp.Core` is deployed alongside the MSBuild task assembly to prevent `FileNotFoundException` when loading `FabulousAstJsonTask` during builds.

### Changed
- Add explicit `PackageReference` to `FSharp.Core` with `PrivateAssets="all"` so it is copied next to `Fabulous.AST.Build.dll` in local bin output and the NuGet `tasks/` folder.

## [2.0.0-pre01] - 2025-12-11
- Initial release

[unreleased]: https://github.com/edgarfgp/Fabulous.AST/compare/2.0.0-pre04...HEAD
[2.0.0-pre04]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre04
[2.0.0-pre03]: https://github.com/edgarfgp/Fabulous.AST/releases/tag/2.0.0-pre03
[2.0.0-pre02]: https://github.com/edgarfgp/Fabulous.AST.Build/releases/tag/2.0.0-pre02
[2.0.0-pre01]: https://github.com/edgarfgp/Fabulous.AST.Build/releases/tag/2.0.0-pre01
