namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InfixApp =

    [<Fact>]
    let ``InfixApp expressions``() =
        Oak() {
            AnonymousModule() {
                InfixAppExpr(ConstantExpr(Constant("a")), "|>", ConstantExpr(Constant("b")))

                PipeRightExpr(ConstantExpr(Constant("a")), ConstantExpr(Constant("b")))

                PipeRightExpr(Constant("a"), Constant("b"))

                PipeRightExpr("a", "b")

                PipeLeftExpr(ConstantExpr(Constant("a")), ConstantExpr(Constant("b")))

                PipeLeftExpr(Constant("a"), Constant("b"))

                PipeLeftExpr("a", "b")

            }
        }
        |> produces
            """
a |> b
a |> b
a |> b
a |> b
a <| b
a <| b
a <| b
"""
