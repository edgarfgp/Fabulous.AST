namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InfixApp =

    [<Fact>]
    let ``InfixApp expressions with pipe operators``() =
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

    [<Fact>]
    let ``InfixApp expressions with various operators``() =
        Oak() {
            AnonymousModule() {
                // Testing basic infix expressions with different operators
                InfixAppExpr(ConstantExpr(Int(1)), "+", ConstantExpr(Int(2)))
                InfixAppExpr(ConstantExpr(Int(3)), "-", ConstantExpr(Int(4)))
                InfixAppExpr(ConstantExpr(Int(5)), "*", ConstantExpr(Int(6)))
                InfixAppExpr(ConstantExpr(Int(7)), "/", ConstantExpr(Int(8)))
                InfixAppExpr(ConstantExpr(Bool(true)), "&&", ConstantExpr(Bool(false)))
                InfixAppExpr(ConstantExpr(Bool(false)), "||", ConstantExpr(Bool(true)))
            }
        }
        |> produces
            """
1 + 2
3 - 4
5 * 6
7 / 8
true && false
false || true
"""

    [<Fact>]
    let ``InfixApp expressions with various parameter combinations``() =
        Oak() {
            AnonymousModule() {
                // Test all the overloads of InfixAppExpr

                // WidgetBuilder<Constant>, string, WidgetBuilder<Expr>
                InfixAppExpr(Int(1), "+", ConstantExpr(Int(2)))

                // string, string, WidgetBuilder<Expr>
                InfixAppExpr("x", "+", ConstantExpr(Int(2)))

                // WidgetBuilder<Constant>, string, WidgetBuilder<Constant>
                InfixAppExpr(Int(3), "*", Int(4))

                // string, string, WidgetBuilder<Constant>
                InfixAppExpr("y", "*", Int(4))

                // string, string, string
                InfixAppExpr("a", "++", "b")

                // WidgetBuilder<Expr>, string, WidgetBuilder<Constant>
                InfixAppExpr(ConstantExpr(Int(5)), "/", Int(2))

                // WidgetBuilder<Constant>, string, string
                InfixAppExpr(Int(6), "^", "2")
            }
        }
        |> produces
            """
1 + 2
x + 2
3 * 4
y * 4
a ++ b
5 / 2
6 ^ 2
"""

    [<Fact>]
    let ``PipeRight expressions with various parameter combinations``() =
        Oak() {
            AnonymousModule() {
                // Test all the overloads of PipeRightExpr

                // WidgetBuilder<Expr>, WidgetBuilder<Expr>
                PipeRightExpr(ConstantExpr(Int(1)), ConstantExpr(Int(2)))

                // WidgetBuilder<Constant>, WidgetBuilder<Expr>
                PipeRightExpr(Int(3), ConstantExpr(Int(4)))

                // string, WidgetBuilder<Expr>
                PipeRightExpr("x", ConstantExpr(Int(5)))

                // WidgetBuilder<Expr>, WidgetBuilder<Constant>
                PipeRightExpr(ConstantExpr(Int(6)), Int(7))

                // WidgetBuilder<Constant>, WidgetBuilder<Constant>
                PipeRightExpr(Int(8), Int(9))

                // WidgetBuilder<Constant>, string
                PipeRightExpr(Int(10), "add")

                // string, string
                PipeRightExpr("value", "toString")
            }
        }
        |> produces
            """
1 |> 2
3 |> 4
x |> 5
6 |> 7
8 |> 9
10 |> add
value |> toString
"""

    [<Fact>]
    let ``PipeLeft expressions with various parameter combinations``() =
        Oak() {
            AnonymousModule() {
                // Test all the overloads of PipeLeftExpr

                // WidgetBuilder<Expr>, WidgetBuilder<Expr>
                PipeLeftExpr(ConstantExpr(Int(1)), ConstantExpr(Int(2)))

                // WidgetBuilder<Constant>, WidgetBuilder<Expr>
                PipeLeftExpr(Int(3), ConstantExpr(Int(4)))

                // WidgetBuilder<Constant>, WidgetBuilder<Constant>
                PipeLeftExpr(Int(5), Int(6))

                // string, WidgetBuilder<Expr>
                PipeLeftExpr("toString", ConstantExpr(Int(7)))

                // WidgetBuilder<Expr>, WidgetBuilder<Constant>
                PipeLeftExpr(ConstantExpr(Int(8)), Int(9))

                // WidgetBuilder<Constant>, string
                PipeLeftExpr(Int(10), "value")

                // string, string
                PipeLeftExpr("toString", "value")
            }
        }
        |> produces
            """
1 <| 2
3 <| 4
5 <| 6
toString <| 7
8 <| 9
10 <| value
toString <| value
"""

    [<Fact>]
    let ``Complex infix expressions``() =
        Oak() {
            AnonymousModule() {
                // Nested infix expressions to test more complex scenarios
                InfixAppExpr(
                    ParenExpr(InfixAppExpr(ConstantExpr(Int(1)), "+", ConstantExpr(Int(2)))),
                    "*",
                    ConstantExpr(Int(3))
                )

                // Mixed pipe operators
                PipeRightExpr(
                    PipeLeftExpr(ConstantExpr(Constant("format")), ConstantExpr(Constant("x"))),
                    ConstantExpr(Constant("print"))
                )
            }
        }
        |> produces
            """
(1 + 2) * 3
format <| x |> print
"""
