namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Constant =

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantString``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(String("a"))) } }
        |> producesValid
            """

let x = "a"
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantMeasure``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(ConstantMeasure(Constant("1.0"), MeasureSingle("cm"))))
            }
        }
        |> producesValid
            """

let x = 1.0<cm>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantMeasure seq``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ConstantExpr(
                        ConstantMeasure(
                            Constant("1.0"),
                            MeasureSeq([ MeasureSingle("cm"); MeasureSingle("/"); MeasureSingle("m") ])
                        )
                    )
                )

                Value("y", ConstantExpr(ConstantMeasure(Constant("1.0"), MeasureSeq([ "cm"; "/"; "m" ]))))
            }
        }
        |> producesValid
            """

let x = 1.0<cm / m>
let y = 1.0<cm / m>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with ConstantUnit``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(ConstantUnit())) } }
        |> producesValid
            """

let x = ()
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureOperator``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ConstantExpr(
                        ConstantMeasure(
                            Constant("55.0f"),
                            MeasureOperator("*", MeasureSingle("miles"), MeasureSingle("hour"))
                        )
                    )
                )
            }
        }
        |> producesValid
            """

let x = 55.0f<miles * hour>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureDivide``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ConstantExpr(
                        ConstantMeasure(Constant("55.0f"), MeasureDivide(MeasureSingle("miles"), MeasureSingle("hour")))
                    )
                )
            }
        }
        |> producesValid
            """

let x = 55.0f<miles / hour>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasureDivide 2``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    InfixAppExpr(
                        ConstantExpr(Constant("55.0f")),
                        "/",
                        ConstantExpr(
                            ConstantMeasure(Constant("1000.0"), MeasureDivide(MeasureSingle("g"), MeasureSingle("kg")))
                        )
                    )
                )
            }
        }
        |> producesValid
            """

let x = 55.0f / 1000.0<g / kg>
"""

    [<Fact>]
    let ``let value with a ConstantExpr expression with MeasurePower``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ConstantExpr(
                        ConstantMeasure(Constant("55.0f"), MeasurePower(MeasureSingle("miles"), Integer("hour")))
                    )
                )
            }
        }
        |> produces
            """

let x = 55.0f<miles^hour>
"""
