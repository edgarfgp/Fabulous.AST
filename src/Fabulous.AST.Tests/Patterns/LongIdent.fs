namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module LongIdent =

    [<Test>]
    let ``let value with a LongIdent pattern`` () =
        AnonymousModule() {
            Value(
                LongIdentPat("x") {
                    NamedPat("B")
                    NamedPat("A")
                },
                ConstantExpr(ConstantString "12")
            )
        }
        |> produces
            """
let x B A = 12
"""

    [<Test>]
    let ``let value with a NamePatPairs pattern with type params`` () =
        AnonymousModule() {
            Value(
                LongIdentPat("x", [ "'a"; "'b" ]) {
                    NamedPat("B")
                    NamedPat("A")
                },
                ConstantExpr(ConstantString "12")
            )
        }
        |> produces
            """

let x<'a, 'b> B A = 12
"""
