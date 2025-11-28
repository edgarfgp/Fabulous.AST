namespace Fabulous.AST.Json.Build

open System

/// Configuration for a single JSON generation item
[<CLIMutable>]
type JsonGenerationItem =
    {
        /// Full path to the JSON input file
        InputPath: string

        /// Full path for the generated output file
        OutputPath: string

        /// Root type name (default: "Root")
        RootName: string

        /// Optional namespace to wrap generated types
        Namespace: string

        /// Optional module name to wrap generated types
        ModuleName: string
    }

module JsonGenerationItem =

    let defaults inputPath outputPath =
        { InputPath = inputPath
          OutputPath = outputPath
          RootName = "Root"
          Namespace = null
          ModuleName = null }

    /// Create a config string for hashing
    let toConfigString(config: JsonGenerationItem) : string =
        sprintf
            "%s|%s|%s"
            config.RootName
            (if isNull config.Namespace then "" else config.Namespace)
            (if isNull config.ModuleName then "" else config.ModuleName)
