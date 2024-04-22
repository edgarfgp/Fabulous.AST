namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Single =
    [<Fact>]
    let ``let value with a Single expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    SingleExpr(
                        SingleNode("a", ConstantExpr(Constant(Unquoted "b")))
                            .addSpace(true)
                            .supportsStroustrup(false)
                    )

                )
            }
        }
        |> produces
            """

let x = a b
"""
