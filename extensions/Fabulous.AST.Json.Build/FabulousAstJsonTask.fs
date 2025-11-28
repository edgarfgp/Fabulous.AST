namespace Fabulous.AST.Json.Build

open System
open System.IO
open Microsoft.Build.Framework
open Microsoft.Build.Utilities
open Fabulous.AST
open Fabulous.AST.Json

/// MSBuild Task that generates F# types from JSON files
type FabulousAstJsonTask() =
    inherit Task()

    // ─────────────────────────────────────────────────────────────────────
    // Input Properties
    // ─────────────────────────────────────────────────────────────────────

    /// JSON files to process (ITaskItem array from MSBuild)
    [<Required>]
    member val JsonFiles: ITaskItem array = Array.empty with get, set

    /// Project directory (for resolving relative paths)
    [<Required>]
    member val ProjectDirectory: string = null with get, set

    /// Intermediate output path (obj/ folder)
    [<Required>]
    member val IntermediateOutputPath: string = null with get, set

    // ─────────────────────────────────────────────────────────────────────
    // Output Properties
    // ─────────────────────────────────────────────────────────────────────

    /// Generated F# files to include in compilation
    [<Output>]
    member val GeneratedFiles: ITaskItem array = Array.empty with get, set

    // ─────────────────────────────────────────────────────────────────────
    // Implementation
    // ─────────────────────────────────────────────────────────────────────

    override this.Execute() =
        try
            let generatedFiles = ResizeArray<ITaskItem>()

            // Create output directory
            let outputDir = Path.Combine(this.IntermediateOutputPath, "FabulousAst")
            Directory.CreateDirectory(outputDir) |> ignore

            for item in this.JsonFiles do
                match this.ProcessItem(item, outputDir) with
                | Some taskItem -> generatedFiles.Add(taskItem)
                | None -> ()

            this.GeneratedFiles <- generatedFiles.ToArray()
            true

        with ex ->
            this.Log.LogErrorFromException(ex, true)
            false

    member private this.ProcessItem(item: ITaskItem, outputDir: string) : ITaskItem option =
        let inputPath =
            let path = item.ItemSpec

            if Path.IsPathRooted(path) then
                path
            else
                Path.Combine(this.ProjectDirectory, path)

        if not(File.Exists(inputPath)) then
            this.Log.LogError("Fabulous.AST.Json: Input file not found: {0}", inputPath)
            None
        else
            // Parse configuration from item metadata
            let config = this.ParseConfig(item, inputPath, outputDir)

            // Read JSON content
            let jsonContent = File.ReadAllText(inputPath)

            // Compute hash for incremental build
            let configString = JsonGenerationItem.toConfigString config
            let hash = HashUtils.computeHash jsonContent configString

            // Check if regeneration needed
            if HashUtils.needsRegeneration config.OutputPath hash then
                this.Log.LogMessage(
                    MessageImportance.Normal,
                    "Fabulous.AST.Json: Generating {0} from {1}",
                    config.OutputPath,
                    inputPath
                )

                // Generate F# code
                let generatedCode = this.GenerateCode(jsonContent, config)

                // Write with header
                let header = HashUtils.generateHeader hash inputPath
                File.WriteAllText(config.OutputPath, header + generatedCode)
            else
                this.Log.LogMessage(
                    MessageImportance.Normal,
                    "Fabulous.AST.Json: Skipping {0} (up-to-date)",
                    config.OutputPath
                )

            Some(TaskItem(config.OutputPath) :> ITaskItem)

    member private this.ParseConfig(item: ITaskItem, inputPath: string, outputDir: string) : JsonGenerationItem =
        let getMetadata (name: string) (defaultValue: string) =
            match item.GetMetadata(name) with
            | null
            | "" -> defaultValue
            | value -> value

        let baseName = Path.GetFileNameWithoutExtension(inputPath)
        let outputFileName = getMetadata "OutputFileName" (baseName + ".Generated.fs")
        let outputPath = Path.Combine(outputDir, outputFileName)

        { InputPath = inputPath
          OutputPath = outputPath
          RootName = getMetadata "RootName" "Root"
          Namespace = item.GetMetadata("Namespace")
          ModuleName = item.GetMetadata("ModuleName") }

    member private this.GenerateCode(jsonContent: string, config: JsonGenerationItem) : string =
        // Build the JSON widget with configuration
        let jsonWidget = Ast.Json(jsonContent).rootName(config.RootName)

        // Build the namespace wrapper or anonymous module
        let oak =
            match config.Namespace with
            | ns when not(String.IsNullOrEmpty(ns)) -> Ast.Oak() { Ast.Namespace(ns) { jsonWidget } }
            | _ -> Ast.Oak() { Ast.AnonymousModule() { jsonWidget } }

        oak |> Gen.mkOak |> Gen.run
