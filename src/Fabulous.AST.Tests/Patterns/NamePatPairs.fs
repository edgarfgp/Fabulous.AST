namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamePatPairsPat =

    [<Fact>]
    let ``let value with a NamePatPairs pattern``() =
        Oak() {
            AnonymousModule() {
                Value(
                    NamePatPairsPat("x", [ NamePatPairPat("A", NamedPat("B")); NamePatPairPat("B", NamedPat("A")) ]),
                    ConstantExpr(Int(12))
                )

                Value(
                    NamePatPairsPat("x", [ NamePatPairPat("A", Constant("B")); NamePatPairPat("B", "A") ]),
                    ConstantExpr(Int(12))
                )
            }
        }
        |> produces
            """

let x (A = B; B = A) = 12
let x (A = B; B = A) = 12
"""

    [<Fact>]
    let ``let value with a NamePatPairs pattern with type params``() =
        Oak() {
            AnonymousModule() {
                Value(
                    NamePatPairsPat("x", [ NamePatPairPat("A", NamedPat("B")); NamePatPairPat("B", NamedPat("A")) ])
                        .typeParams([ "'a"; "'b" ]),
                    ConstantExpr(Int(12))
                )
            }
        }
        |> produces
            """

let x<'a, 'b> (A = B; B = A) = 12
"""
