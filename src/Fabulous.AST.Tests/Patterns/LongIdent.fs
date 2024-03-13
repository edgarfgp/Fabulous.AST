namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module LongIdent =

    [<Fact>]
    let ``let value with a LongIdent pattern``() =
        AnonymousModule() {
            Value(
                LongIdentPat("x") {
                    NamedPat("B")
                    NamedPat("A")
                },
                ConstantExpr(Constant("12").hasQuotes(false))
            )
        }
        |> produces
            """
let x B A = 12
"""

    [<Fact>]
    let ``let value with a NamePatPairs pattern with type params``() =
        AnonymousModule() {
            Value(
                LongIdentPat("x", [ "'a"; "'b" ]) {
                    NamedPat("B")
                    NamedPat("A")
                },
                ConstantExpr(Constant("12").hasQuotes(false))
            )
        }
        |> produces
            """

let x<'a, 'b> B A = 12
"""
