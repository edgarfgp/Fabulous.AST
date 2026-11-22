namespace Fabulous.AST.Tests.Core

open System
open Fabulous.AST
open Xunit

module StaticInitializationTests =

    /// Modules hold the widget and attribute keys. If a module gets its own static constructor (as it
    /// does when the library is compiled with `--realsig+`), modules that share a file initialize each
    /// other through the file's startup class, and two threads using them for the first time can
    /// deadlock. The runtime then lets one of them read keys that are still 0, which shows up as
    /// "Could not find widget collection attribute Decls on widget { Key = 0 ... }". Without
    /// per-module static constructors, each file has a single initializer and the cycle cannot form.
    [<Fact>]
    let ``F# modules of Fabulous.AST have no static constructor``() =
        let isModule(t: Type) =
            t.GetCustomAttributes(typeof<CompilationMappingAttribute>, false)
            |> Array.exists(fun a ->
                (a :?> CompilationMappingAttribute).SourceConstructFlags = SourceConstructFlags.Module)

        let offenders =
            typeof<WidgetBuilder<obj>>.Assembly.GetTypes()
            |> Array.filter(fun t -> isModule t && not(isNull t.TypeInitializer))
            |> Array.map _.FullName
            |> Array.sort

        Assert.True(
            Array.isEmpty offenders,
            "These modules have a static constructor, so concurrent first use can read uninitialized widget keys "
            + "(is the library compiled with --realsig+?):\n"
            + String.Join("\n", offenders)
        )
