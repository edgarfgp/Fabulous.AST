namespace Fabulous.AST.Tests.Core

open Fantomas.Core
open NUnit.Framework

open Fantomas.Core.SyntaxOak

open Fabulous.AST

open type Fabulous.AST.Ast

module CollectionBuilder =

    [<Test>]
    let ``stress test`` () =
        let result =
            Namespace("DummyNamespace").isRecursive() {
                Abbrev($"Foo", Type.FromString "string")
                Abbrev($"bar", Type.FromString "string")

                for i = 0 to 10 do
                    Abbrev($"T{i}", Type.FromString "string")

                for i = 10 to 20 do
                    Abbrev($"T{i}", Type.FromString "string")

                for i = 20 to 30 do
                    Abbrev($"T{i}", Type.FromString "string")
            }
            |> Tree.compile
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result

    [<Test>]
    let ``stress test2`` () =
        let result =
            Namespace("DummyNamespace").isRecursive() {
                for i = 0 to 10 do
                    Abbrev($"T{i}", Type.FromString "string")

                for i = 10 to 20 do
                    Abbrev($"T{i}", Type.FromString "string")

                for i = 20 to 30 do
                    Abbrev($"T{i}", Type.FromString "string")
            }
            |> Tree.compile
            |> CodeFormatter.FormatOakAsync
            |> Async.RunSynchronously

        Assert.NotNull result
