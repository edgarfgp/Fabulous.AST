namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TuplePat =

    [<Test>]
    let ``let value with a Tuple pattern`` () =
        AnonymousModule() {
            Value(
                TuplePat() {
                    NamedPat("a")
                    NamedPat("b")
                },
                ConstantExpr(ConstantString "12")
            )
        }
        |> produces
            """

let a, b = 12
"""
