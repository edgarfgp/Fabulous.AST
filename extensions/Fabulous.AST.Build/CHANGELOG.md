# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0-pre2] - 2025-12-12

### Fixed
- Ensure `FSharp.Core` is deployed alongside the MSBuild task assembly to prevent `FileNotFoundException` when loading `FabulousAstJsonTask` during builds.

### Changed
- Add explicit `PackageReference` to `FSharp.Core` with `PrivateAssets="all"` so it is copied next to `Fabulous.AST.Build.dll` in local bin output and the NuGet `tasks/` folder.

## [2.0.0-pre01] - 2025-12-11
- Initial release

[unreleased]: https://github.com/edgarfgp/Fabulous.AST.Build/compare/2.0.0-pre02...HEAD
[2.0.0-pre02]: https://github.com/edgarfgp/Fabulous.AST.Build/releases/tag/2.0.0-pre02
[2.0.0-pre01]: https://github.com/edgarfgp/Fabulous.AST.Build/releases/tag/2.0.0-pre01
