namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AppSingleParenArg =

    [<Fact>]
    let ``let value with a AppSingleParenArg expression``() =
        let config: Fantomas.Core.FormatConfig =
            { Fantomas.Core.FormatConfig.Default with
                SpaceBeforeLowercaseInvocation = false
                SpaceBeforeUppercaseInvocation = true }

        Oak() {
            AnonymousModule() {
                AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ "0"; "0"; "0" ])))

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
        |> producesWithConfig
            config
            """
MyClass (0, 0, 0)
x?a("")?b(t)?b(t)
"""
