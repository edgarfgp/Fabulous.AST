namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Constant =

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantString``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr("a")) } }
        |> produces
            """

let x = "a"
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantMeasure``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(ConstantMeasure(Constant("1.0").hasQuotes(false), MeasureSingle("cm"))))
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
                            Constant("55.0f").hasQuotes(false),
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
                            Constant("55.0f").hasQuotes(false),
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
                        ConstantExpr(Constant("55.0f").hasQuotes(false)),
                        "/",
                        ConstantExpr(
                            ConstantMeasure(
                                Constant("1000.0").hasQuotes(false),
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
                            Constant("55.0f").hasQuotes(false),
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
