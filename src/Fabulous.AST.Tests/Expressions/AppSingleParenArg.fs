namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AppSingleParenArg =

    [<Fact>]
    let ``AppSingleParenArg expression``() =
        Oak() {
            AnonymousModule() {
                AppSingleParenArgExpr(
                    DynamicExpr(
                        AppSingleParenArgExpr(
                            DynamicExpr(AppSingleParenArgExpr(DynamicExpr("x", "a"), ParenExpr("")), "b"),
                            ParenExpr("t")
                        ),
                        "b"
                    ),
                    ParenExpr("t")
                )
            }
        }
        |> produces
            """
MyClass (0, 0, 0)
x?a("")?b(t)?b(t)
"""
