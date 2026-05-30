#r "nuget: Fun.Build, 1.0.3"

open System
open System.IO
open Fun.Build

let (</>) a b = Path.Combine(a, b)
let sln = __SOURCE_DIRECTORY__ </> "Fabulous.AST.sln"
let config = "Release"
let nupkgs = __SOURCE_DIRECTORY__ </> "nupkgs"

let nightlyVersion =
    Environment.GetEnvironmentVariable("NIGHTLY_VERSION")
    |> Option.ofObj
    |> Option.bind(fun nv -> if String.IsNullOrWhiteSpace(nv) then None else Some nv)

let versionProperty =
    match nightlyVersion with
    | None -> String.Empty
    | Some nv -> $"-p:Version=%s{nv}"

pipeline "ci" {
    description "Main pipeline used during pull requests"

    stage "lint" {
        run "dotnet tool restore"
        run $"dotnet fantomas --check {__SOURCE_FILE__} src docs extensions"
    }

    stage "build" {
        run $"dotnet restore {sln}"
        // Fabulous.AST.Build ships an MSBuild task that samples/Playground consumes
        // via <UsingTask> in Fabulous.AST.Build.targets. UsingTask conditions are
        // evaluated at project-load time, so the task DLL must exist on disk before
        // we build anything that imports those targets. Build it first.
        run $"dotnet build extensions/Fabulous.AST.Build -c {config} --no-restore"
        run $"dotnet build {sln} -c {config} --no-restore"
        run $"dotnet test {sln} -c {config} --no-build"
    }

    stage "docs" {
        run $"dotnet publish src/Fabulous.AST -f netstandard2.1 -c {config}"
        run $"dotnet publish extensions/Fabulous.AST.Json -f net8.0 -c {config}"
        run $"dotnet publish extensions/Fabulous.AST.Build -f net8.0 -c {config}"
        run $"dotnet fsdocs build --properties Configuration={config} --eval --strict"
    }

    stage "pack" { run $"dotnet pack {sln} -c {config} -p:PackageOutputPath=\"%s{nupkgs}\" {versionProperty}" }

    runIfOnlySpecified false
}

pipeline "docs" {
    description "Run the documentation website"

    stage "build" {
        run "dotnet publish src/Fabulous.AST -f netstandard2.1 -c Release"
        run "dotnet publish extensions/Fabulous.AST.Json -f net8.0 -c Release"
        run "dotnet publish extensions/Fabulous.AST.Build -f net8.0 -c Release"
    }

    stage "watch" { run "dotnet fsdocs watch --eval --clean" }
    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp()
