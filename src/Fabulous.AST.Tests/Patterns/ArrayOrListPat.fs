namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrListPat =

    [<Test>]
    let ``let value with a List pattern`` () =
        AnonymousModule() {
            Value(
                ListPat() {
                    NamedPat("a")
                    NamedPat("b")
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let [ a; b ] = 12
"""

    [<Test>]
    let ``let value with an Array pattern`` () =
        AnonymousModule() {
            Value(
                ArrayPat() {
                    NamedPat("a")
                    NamedPat("b")
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """
let [| a; b |] = 12
"""
