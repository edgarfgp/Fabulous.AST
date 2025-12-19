namespace Fabulous.AST.Build

open System
open System.IO
open Microsoft.Build.Framework
open Microsoft.Build.Utilities
open Fabulous.AST
open Fabulous.AST.Json
open type Fabulous.AST.Ast

/// MSBuild Task that generates F# types from JSON files
type FabulousAstJsonTask() =
    inherit Task()

    // Input Properties

    /// JSON files to process (ITaskItem array from MSBuild)
    [<Required>]
    member val JsonFiles: ITaskItem array = Array.empty with get, set

    /// Project directory (for resolving relative paths)
    [<Required>]
    member val ProjectDirectory: string = null with get, set

    /// Output directory for generated files
    [<Required>]
    member val OutputDirectory: string = null with get, set

    [<Required>]
    member val FabulousAstJsonSuffix: string = null with get, set

    // Output Properties

    /// Generated F# files to include in compilation
    [<Output>]
    member val GeneratedFiles: ITaskItem array = Array.empty with get, set

    // Implementation

    override this.Execute() =
        try
            let generatedFiles = ResizeArray<ITaskItem>()

            // Create an output directory
            Directory.CreateDirectory(this.OutputDirectory) |> ignore

            for item in this.JsonFiles do
                match this.ProcessItem(item, this.OutputDirectory) with
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
            let config = this.ParseConfig(item, inputPath, outputDir)
            let jsonContent = File.ReadAllText(inputPath)
            let configString = JsonGenerationItem.toConfigString config
            let hash = HashUtils.computeHash jsonContent configString

            if HashUtils.needsRegeneration config.OutputPath hash then
                this.Log.LogMessage(
                    MessageImportance.Normal,
                    "Fabulous.AST.Json: Generating {0} from {1}",
                    config.OutputPath,
                    inputPath
                )

                let generatedCode = this.GenerateCode(jsonContent, config)
                let relativeInputPath = Path.GetRelativePath(this.ProjectDirectory, inputPath)
                let header = HashUtils.generateHeader hash relativeInputPath inputPath
                File.WriteAllText(config.OutputPath, header + generatedCode)
            else
                this.Log.LogMessage(
                    MessageImportance.Low,
                    "Fabulous.AST.Json: Skipping {0} (up-to-date)",
                    config.OutputPath
                )

            Some(TaskItem(config.OutputPath) :> ITaskItem)

    member private x.ParseConfig(item: ITaskItem, inputPath: string, outputDir: string) : JsonGenerationItem =
        let getMetadata (name: string) (defaultValue: string) =
            match item.GetMetadata(name) with
            | null
            | "" -> defaultValue
            | value -> value

        let baseName = Path.GetFileNameWithoutExtension(inputPath)

        let outputFileName =
            match getMetadata "OutputFileName" "" with
            | "" -> baseName + $".{x.FabulousAstJsonSuffix}.fs"
            | name -> name

        let outputPath = Path.Combine(outputDir, outputFileName)

        { InputPath = inputPath
          OutputPath = outputPath
          RootName = getMetadata "RootName" "Root"
          ModuleName = item.GetMetadata("ModuleName") }

    member private _.GenerateCode(jsonContent: string, config: JsonGenerationItem) : string =
        let jsonWidget = Json(jsonContent).rootName(config.RootName)

        // Only support module-based codegen (module A.B). If ModuleName is empty, use anonymous module.
        let hasMod = not(String.IsNullOrWhiteSpace(config.ModuleName))

        let oak =
            if hasMod then
                // Use file-level module (emits: module A.B)
                Oak() { (Namespace(config.ModuleName) { jsonWidget }).toImplicit() }
            else
                Oak() { AnonymousModule() { jsonWidget } }

        oak |> Gen.mkOak |> Gen.run
