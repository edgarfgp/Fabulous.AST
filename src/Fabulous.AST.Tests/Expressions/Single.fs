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
                    ConstantPat(Constant("x")),
                    SingleExpr(
                        SingleNode("a", ConstantExpr(Constant("b")))
                            .addSpace(true)
                            .supportsStroustrup(false)
                    )

                )

                Value(
                    ConstantPat(Constant("x")),
                    SingleExpr(SingleNode("a", Constant("b")).addSpace(true).supportsStroustrup(false))
                )

                Value(
                    ConstantPat(Constant("x")),
                    SingleExpr(SingleNode("a", "b").addSpace(true).supportsStroustrup(false))
                )
            }
        }
        |> produces
            """

let x = a b
let x = a b
let x = a b
"""
