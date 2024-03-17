namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Constant =

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantString``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Quoted "a")) } }
        |> produces
            """

let x = "a"
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantMeasure``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(ConstantMeasure(Constant(Unquoted "1.0"), MeasureSingle("cm"))))
            }
        }
        |> produces
            """

let x = 1.0<cm>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantUnit``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(ConstantUnit())) } }
        |> produces
            """

let x = ()
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureOperator``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(
                        ConstantMeasure(
                            Constant(Unquoted "55.0f"),
                            MeasureOperator("*", MeasureSingle("miles"), MeasureSingle("hour"))
                        )
                    )
                )
            }
        }
        |> produces
            """

let x = 55.0f<miles * hour>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureDivide``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(
                        ConstantMeasure(
                            Constant(Unquoted "55.0f"),
                            MeasureDivide("/", MeasureSingle("miles"), MeasureSingle("hour"))
                        )
                    )
                )
            }
        }
        |> produces
            """

let x = 55.0f<miles / hour>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureDivide 2``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    InfixAppExpr(
                        ConstantExpr(Constant(Unquoted "55.0f")),
                        "/",
                        ConstantExpr(
                            ConstantMeasure(
                                Constant(Unquoted "1000.0"),
                                MeasureDivide("/", MeasureSingle("g"), MeasureSingle("kg"))
                            )
                        )
                    )
                )
            }
        }
        |> produces
            """

let x = 55.0f / 1000.0<g / kg>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasurePower``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(
                        ConstantMeasure(
                            Constant(Unquoted "55.0f"),
                            MeasurePower("*", MeasureSingle("miles"), Integer("hour"))
                        )
                    )
                )
            }
        }
        |> produces
            """

let x = 55.0f<miles*hour>
"""
