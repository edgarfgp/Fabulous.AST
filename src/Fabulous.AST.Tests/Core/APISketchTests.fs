namespace Fabulous.AST.Tests.Core

open System
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core
open NUnit.Framework

open Fantomas.Core.SyntaxOak

open Fabulous.AST

open type Fabulous.AST.Ast

module APISketchTests =

    [<Test>]
    let ``Multiple Widgets for loops in builder`` () =
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
    let ``Multiple for loops in builder`` () =
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
