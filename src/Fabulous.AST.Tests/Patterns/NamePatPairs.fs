namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamePatPairsPat =

    [<Test>]
    let ``let value with a NamePatPairs pattern`` () =
        AnonymousModule() {
            Value(
                NamePatPairsPat("x") {
                    NamePatPairPat("A", NamedPat("B"))
                    NamePatPairPat("B", NamedPat("A"))
                },
                ConstantExpr(Constant "12")
            )
        }
        |> produces
            """

let x (A = B; B = A) = 12
"""

    [<Test>]
    let ``let value with a NamePatPairs pattern with type params`` () =
        AnonymousModule() {
            Value(
                NamePatPairsPat("x", [ "'a"; "'b" ]) {
                    NamePatPairPat("A", NamedPat("B"))
                    NamePatPairPat("B", NamedPat("A"))
                },
                ConstantExpr(Constant "12")
            )
        }
        |> produces
            """

let x<'a, 'b> (A = B; B = A) = 12
"""
