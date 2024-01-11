namespace Fabulous.AST.Tests.Core

open Fantomas.Core
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module APISketchTests =

    [<Test>]
    let ``Multiple Widgets for loops in builder`` () =
        let result =
            RecNamespace("DummyNamespace") {
                Abbrev("Foo", CommonType.String)
                Abbrev("bar", CommonType.String)

                for i = 0 to 10 do
                    Abbrev($"T{i}", CommonType.String)

                for i = 10 to 20 do
                    Abbrev($"T{i}", CommonType.String)

                for i = 20 to 30 do
                    Abbrev($"T{i}", CommonType.String)
            }
            |> Tree.compile
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result

    [<Test>]
    let ``Multiple for loops in builder`` () =
        let result =
            RecNamespace("DummyNamespace") {
                for i = 0 to 10 do
                    Abbrev($"T{i}", CommonType.String)

                for i = 10 to 20 do
                    Abbrev($"T{i}", CommonType.String)

                for i = 20 to 30 do
                    Abbrev($"T{i}", CommonType.String)
            }
            |> Tree.compile
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result
