namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module UnitsOfMeasure =

    [<Test>]
    let ``Produces type Unit of measure`` () =
        AnonymousModule() { UnitsOfMeasure("cm") }

        |> produces
            """

[<Measure>]
type cm
"""
