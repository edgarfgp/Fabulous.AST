namespace Fabulous.AST.Tests.Core

open Fantomas.Core
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module APISketchTests =

    [<Test>]
    let ``Multiple Widgets for loops in builder`` () =
        let result =
            Namespace("DummyNamespace") {
                Abbrev("Foo", String())
                Abbrev("bar", String())

                for i = 0 to 10 do
                    Abbrev($"T{i}", String())

                for i = 10 to 20 do
                    Abbrev($"T{i}", String())

                for i = 20 to 30 do
                    Abbrev($"T{i}", String())
            }
            |> Gen.mkOak
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result

    [<Test>]
    let ``Multiple for loops in builder`` () =
        let result =
            Namespace("DummyNamespace") {
                for i = 0 to 10 do
                    Abbrev($"T{i}", String())

                for i = 10 to 20 do
                    Abbrev($"T{i}", String())

                for i = 20 to 30 do
                    Abbrev($"T{i}", String())
            }
            |> Gen.mkOak
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result
