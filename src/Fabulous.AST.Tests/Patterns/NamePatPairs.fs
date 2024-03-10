namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamePatPairsPat =

    [<Fact>]
    let ``let value with a NamePatPairs pattern`` () =
        AnonymousModule() {
            Value(
                NamePatPairsPat("x") {
                    NamePatPairPat("A", NamedPat("B"))
                    NamePatPairPat("B", NamedPat("A"))
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let x (A = B; B = A) = 12
"""

    [<Fact>]
    let ``let value with a NamePatPairs pattern with type params`` () =
        AnonymousModule() {
            Value(
                NamePatPairsPat("x", [ "'a"; "'b" ]) {
                    NamePatPairPat("A", NamedPat("B"))
                    NamePatPairPat("B", NamedPat("A"))
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let x<'a, 'b> (A = B; B = A) = 12
"""
