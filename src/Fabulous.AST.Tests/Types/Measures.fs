namespace Fabulous.AST.Tests.Types

open Xunit
open Fabulous.AST.Tests
open Fabulous.AST

open type Ast

module Measures =

    [<Fact>]
    let ``Basic measure``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Constant("1.0"), MeasureSingle("cm"))) } }
        |> produces
            """
1.0<cm>
"""

    [<Fact>]
    let ``Integer with measure``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Int(42), MeasureSingle("kg"))) } }
        |> produces
            """
42<kg>
"""

    [<Fact>]
    let ``Float with measure``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Float(3.14), MeasureSingle("rad"))) } }
        |> produces
            """
3.14<rad>
"""

    [<Fact>]
    let ``Decimal with measure``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Decimal(9.99m), MeasureSingle("$"))) } }
        |> produces
            """
9.99m<$>
"""

    [<Fact>]
    let ``Measure division``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Float(55.0), MeasureDivide("km", "h"))) } }
        |> produces
            """
55.0<km / h>
"""

    [<Fact>]
    let ``Measure multiplication (operator)``() =
        Oak() { AnonymousModule() { ConstantExpr(ConstantMeasure(Int(10), MeasureOperator("*", "N", "m"))) } }
        |> produces
            """
10<N * m>
"""

    [<Fact>]
    let ``Measure sequence``() =
        Oak() {
            AnonymousModule() {
                ConstantExpr(ConstantMeasure(Double(42.0), MeasureSeq([ "kg"; "*"; "m"; "/"; "s"; "^"; "2" ])))
            }
        }
        |> produces
            """
42.0<kg * m / s ^ 2>
"""

    [<Fact>]
    let ``Measure power (integer)``() =
        Oak() {
            AnonymousModule() {
                ConstantExpr(ConstantMeasure(Float(100.0), MeasurePower(MeasureSingle("m"), Integer("2"))))
            }
        }
        |> produces
            """
100.0<m^2>
"""

    [<Fact>]
    let ``Measure power (negative)``() =
        Oak() {
            AnonymousModule() {
                ConstantExpr(ConstantMeasure(Float(10.0), MeasureSeq([ MeasurePower("s", Negate(Integer("1"))) ])))
            }
        }
        |> produces
            """
10.0<s^-1>
"""

    [<Fact>]
    let ``Measure power (rational)``() =
        Oak() {
            AnonymousModule() { ConstantExpr(ConstantMeasure(Float(2.0), MeasurePower("L", Rational("1", "/", "2")))) }
        }
        |> produces
            """
2.0<L^(1/2)>
"""

    [<Fact>]
    let ``Measure with parentheses``() =
        Oak() {
            AnonymousModule() {
                ConstantExpr(
                    ConstantMeasure(Double(5.0), MeasureParen(MeasureDivide(MeasureSingle("J"), MeasureSingle("K"))))
                )
            }
        }
        |> produces
            """
5.0<(J / K)>
"""

    [<Fact>]
    let ``Multiple measure types in one module``() =
        Oak() {
            AnonymousModule() {
                // Define custom measures
                Measure("m")
                Measure("s")

                // Simple single measures
                Value("length", ConstantExpr(ConstantMeasure(Float(10.0), "m")), AppPrefix(Float(), "m"))

                Value("time", ConstantExpr(ConstantMeasure(Float(5.0), "s")), AppPrefix(Float(), "s"))

                // Derived measures
                Value("speed", InfixAppExpr("length", "/", "time"), AppPrefix(Float(), Tuple([ "m"; "s" ], "/")))

                // Complex measures with powers
                Value(
                    "area",
                    ConstantExpr(ConstantMeasure(Float(50.0), MeasurePower("m", Integer(2)))),
                    AppPrefix(Float(), MeasurePowerType("m", Integer(2)))
                )

                Value(
                    "acceleration",
                    ConstantExpr(ConstantMeasure(Float(9.81), MeasureDivide("m", MeasurePower("s", Integer(2))))),
                    AppPrefix(Float(), Tuple([ LongIdent "m"; MeasurePowerType("s", Integer("2")) ], "/"))

                )
            }
        }
        |> produces
            """
[<Measure>]
type m

[<Measure>]
type s

let length: float<m> = 10.0<m>
let time: float<s> = 5.0<s>
let speed: float<m / s> = length / time
let area: float<m^2> = 50.0<m^2>
let acceleration: float<m / s^2> = 9.81<m / s^2>
"""

    [<Fact>]
    let ``Predefined measures``() =
        Oak() {
            AnonymousModule() {
                // Define measure types
                Measure("s")
                Measure("min")

                // Time units
                Value("seconds", ConstantExpr(ConstantMeasure(Float(1.0), "s")), AppPrefix(Float(), "s"))

                Value("minutes", ConstantExpr(ConstantMeasure(Float(60.0), "s")), AppPrefix(Float(), "s"))

                Value("hours", ConstantExpr(ConstantMeasure(Float(3600.0), "s")), AppPrefix(Float(), "s"))

                // Convert between units
                Value("minutesFromSeconds", InfixAppExpr("seconds", "/", Float(60.0)), AppPrefix(Float(), "min"))
            }
        }
        |> produces
            """
[<Measure>]
type s

[<Measure>]
type min

let seconds: float<s> = 1.0<s>
let minutes: float<s> = 60.0<s>
let hours: float<s> = 3600.0<s>
let minutesFromSeconds: float<min> = seconds / 60.0
"""

    [<Fact>]
    let ``Define custom measure types``() =
        Oak() {
            AnonymousModule() {
                // Define custom measures with the [< Measure >] attribute
                Measure("m")

                Measure("s")

                Measure("kg")

                // Define derived measure
                Measure("N") |> _.xmlDocs([ "Newton: kg·m/s²" ])

                // Use custom measures
                Value("mass", Float(10.0), AppPrefix(Float(), "kg"))

                Value(
                    "acceleration",
                    Float(9.81),
                    AppPrefix(Float(), Tuple([ LongIdent "m"; MeasurePowerType("s", Integer("2")) ], "/"))
                )

                Value("force", InfixAppExpr("mass", "*", "acceleration"), AppPrefix(Float(), "N"))
            }
        }
        |> produces
            """
[<Measure>]
type m

[<Measure>]
type s

[<Measure>]
type kg

/// Newton: kg·m/s²
[<Measure>]
type N

let mass: float<kg> = 10.0
let acceleration: float<m / s^2> = 9.81
let force: float<N> = mass * acceleration
"""
