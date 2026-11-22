#r "nuget: Fun.Build, 1.0.3"

open System
open System.IO
open Fun.Build

let (</>) a b = Path.Combine(a, b)
let sln = __SOURCE_DIRECTORY__ </> "Fabulous.AST.slnx"
let config = "Release"
let nupkgs = __SOURCE_DIRECTORY__ </> "nupkgs"

/// The packages whose API reference fsdocs generates. Listed explicitly: fsdocs 22 reads the .slnx
/// and would otherwise crack every project in it (samples, tests), failing under --strict.
let docsProjects =
    [ "src/Fabulous.AST/Fabulous.AST.fsproj"
      "extensions/Fabulous.AST.Json/Fabulous.AST.Json.fsproj"
      "extensions/Fabulous.AST.Build/Fabulous.AST.Build.fsproj" ]
    |> String.concat " "

let nightlyVersion =
    Environment.GetEnvironmentVariable("NIGHTLY_VERSION")
    |> Option.ofObj
    |> Option.bind(fun nv -> if String.IsNullOrWhiteSpace(nv) then None else Some nv)

let versionProperty =
    match nightlyVersion with
    | None -> String.Empty
    | Some nv -> $"-p:Version=%s{nv}"

/// `fsdocs build --eval --strict` fails on compile errors, but a snippet that throws at run time
/// just renders an empty `include-output` block. Fail the build on those instead.
let checkDocsOutput(siteDir: string) : unit =
    let emptyOutput =
        Text.RegularExpressions.Regex(@"<table class=""pre""><tr><td><pre><code>\s*</code></pre>")

    let offenders =
        Directory.GetFiles(siteDir, "*.html", SearchOption.AllDirectories)
        |> Array.choose(fun file ->
            let count = emptyOutput.Matches(File.ReadAllText file).Count

            if count > 0 then
                Some $"  {Path.GetRelativePath(siteDir, file)}: {count} empty output block(s)"
            else
                None)

    if offenders.Length > 0 then
        failwith(
            "These docs pages have empty evaluated output; the snippet most likely threw at run time:\n"
            + String.Join("\n", offenders)
        )

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
        run $"dotnet publish extensions/Fabulous.AST.Json -f net10.0 -c {config}"
        run $"dotnet publish extensions/Fabulous.AST.Build -f net10.0 -c {config}"
        run $"dotnet fsdocs build --projects {docsProjects} --properties Configuration={config} --eval --strict"
        run(fun _ -> checkDocsOutput(__SOURCE_DIRECTORY__ </> "output"))
    }

    stage "pack" { run $"dotnet pack {sln} -c {config} -p:PackageOutputPath=\"%s{nupkgs}\" {versionProperty}" }

    runIfOnlySpecified false
}

pipeline "docs" {
    description "Run the documentation website"

    stage "build" {
        run "dotnet publish src/Fabulous.AST -f netstandard2.1 -c Release"
        run "dotnet publish extensions/Fabulous.AST.Json -f net10.0 -c Release"
        run "dotnet publish extensions/Fabulous.AST.Build -f net10.0 -c Release"
    }

    stage "watch" { run $"dotnet fsdocs watch --projects {docsProjects} --eval --clean" }
    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp()
