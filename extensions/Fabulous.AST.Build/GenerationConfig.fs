namespace Fabulous.AST.Build

/// Configuration for a single JSON generation item
type JsonGenerationItem =
    { InputPath: string
      OutputPath: string
      RootName: string
      ModuleName: string }

module JsonGenerationItem =
    /// Create a config string for hashing (parameters that affect code shape)
    let toConfigString(config: JsonGenerationItem) =
        sprintf "%s|%s" config.RootName (config.ModuleName |> Option.ofObj |> Option.defaultValue "")
