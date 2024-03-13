namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Single =
    [<Fact>]
    let ``let value with a Single expression``() =
        AnonymousModule() {
            Value(
                "x",
                SingleExpr("a", ConstantExpr(Constant("b").hasQuotes(false)))
                    .addSpace(true)
                    .supportsStroustrup(false)
            )
        }
        |> produces
            """

let x = a b
"""
