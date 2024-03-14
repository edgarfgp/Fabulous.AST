namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrListPat =

    [<Fact>]
    let ``let value with a List pattern``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ListPat() {
                        NamedPat("a")
                        NamedPat("b")
                    },
                    ConstantExpr(Constant("12").hasQuotes(false))
                )
            }
        }
        |> produces
            """

let [ a; b ] = 12
"""

    [<Fact>]
    let ``let value with an Array pattern``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ArrayPat() {
                        NamedPat("a")
                        NamedPat("b")
                    },
                    ConstantExpr(Constant("12").hasQuotes(false))
                )
            }
        }
        |> produces
            """
let [| a; b |] = 12
"""
