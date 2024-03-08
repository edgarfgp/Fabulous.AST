namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Constant =

    [<Test>]
    let ``let value with a ConstantExpr expression with ConstantString`` () =
        AnonymousModule() { Value("x", ConstantExpr("a")) }
        |> produces
            """

let x = "a"
"""

    [<Test>]
    let ``let value with a ConstantExpr expression with ConstantMeasure`` () =
        AnonymousModule() { Value("x", ConstantExpr(ConstantMeasure(Constant("1.0", false), MeasureSingle("cm")))) }
        |> produces
            """

let x = 1.0<cm>
"""

    [<Test>]
    let ``let value with a ConstantExpr expression with ConstantUnit`` () =
        AnonymousModule() { Value("x", ConstantExpr(ConstantUnit())) }
        |> produces
            """

let x = ()
"""

    [<Test>]
    let ``let value with a ConstantExpr expression with MeasureOperator`` () =
        AnonymousModule() {
            Value(
                "x",
                ConstantExpr(
                    ConstantMeasure(
                        Constant("55.0f", false),
                        MeasureOperator("*", MeasureSingle("miles"), MeasureSingle("hour"))
                    )
                )
            )
        }
        |> produces
            """

let x = 55.0f<miles * hour>
"""

    [<Test>]
    let ``let value with a ConstantExpr expression with MeasureDivide`` () =
        AnonymousModule() {
            Value(
                "x",
                ConstantExpr(
                    ConstantMeasure(
                        Constant("55.0f", false),
                        MeasureDivide("/", MeasureSingle("miles"), MeasureSingle("hour"))
                    )
                )
            )
        }
        |> produces
            """

let x = 55.0f<miles / hour>
"""

    [<Test>]
    let ``let value with a ConstantExpr expression with MeasureDivide 2`` () =
        AnonymousModule() {
            Value(
                "x",
                InfixAppExpr(
                    ConstantExpr(Constant("55.0f", false)),
                    "/",
                    ConstantExpr(
                        ConstantMeasure(
                            Constant("1000.0", false),
                            MeasureDivide("/", MeasureSingle("g"), MeasureSingle("kg"))
                        )
                    )
                )
            )
        }
        |> produces
            """

let x = 55.0f / 1000.0<g / kg>
"""


    [<Test>]
    let ``let value with a ConstantExpr expression with MeasurePower`` () =
        AnonymousModule() {
            Value(
                "x",
                ConstantExpr(
                    ConstantMeasure(
                        Constant("55.0f", false),
                        MeasurePower("*", MeasureSingle("miles"), Integer("hour"))
                    )
                )
            )
        }
        |> produces
            """

let x = 55.0f<miles*hour>
"""
