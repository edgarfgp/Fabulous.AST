namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Ands =

    [<Test>]
    let ``let value with a Ands pattern`` () =
        AnonymousModule() {
            Value(
                AndsPat() {
                    NamedPat("A")
                    NamedPat("B")
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let A & B = 12
"""
