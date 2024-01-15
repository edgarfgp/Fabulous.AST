namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Type


module UnitsOfMeasure =

    [<Test>]
    let ``Produces type Unit of measure`` () =
        AnonymousModule() {
            UnitsOfMeasure("cm").xmlDocs([ "Cm, centimeters." ])

            UnitsOfMeasure("ml", MeasurePower([ "cm" ], "3"))
                .xmlDocs([ "Ml, milliliters." ])

            UnitsOfMeasure("g").xmlDocs([ "Mass, grams." ])
            UnitsOfMeasure("kg").xmlDocs([ "Mass, kilograms." ])
            UnitsOfMeasure("lb").xmlDocs([ "Weight, pounds." ])
            UnitsOfMeasure("m").xmlDocs([ "Distance, meters." ])
            UnitsOfMeasure("cm").xmlDocs([ "Distance, cm" ])
            UnitsOfMeasure("inch").xmlDocs([ "Distance, inches." ])
            UnitsOfMeasure("ft").xmlDocs([ "Distance, feet" ])
            UnitsOfMeasure("s").xmlDocs([ "Time, seconds." ])

            UnitsOfMeasure("N", Tuple([ "kg" ], [ "m" ], MeasurePower([ "s" ], "2")))
                .xmlDocs([ "Force, Newtons." ])

            UnitsOfMeasure("bar").xmlDocs([ "Pressure, bar." ])

            UnitsOfMeasure("Pa", Tuple([ "N" ], [], MeasurePower([ "m" ], "2")))
                .xmlDocs([ "Pressure, Pascals" ])

            UnitsOfMeasure("ml").xmlDocs([ "Volume, milliliters." ])

            UnitsOfMeasure("L").xmlDocs([ "Volume, liters." ])
        }

        |> produces
            """

/// Cm, centimeters.
[<Measure>]
type cm

/// Ml, milliliters.
[<Measure>]
type ml = cm^3

/// Mass, grams.
[<Measure>]
type g

/// Mass, kilograms.
[<Measure>]
type kg

/// Weight, pounds.
[<Measure>]
type lb

/// Distance, meters.
[<Measure>]
type m

/// Distance, cm
[<Measure>]
type cm

/// Distance, inches.
[<Measure>]
type inch

/// Distance, feet
[<Measure>]
type ft

/// Time, seconds.
[<Measure>]
type s

/// Force, Newtons.
[<Measure>]
type N = kg m / s^2

/// Pressure, bar.
[<Measure>]
type bar

/// Pressure, Pascals
[<Measure>]
type Pa = N / m^2

/// Volume, milliliters.
[<Measure>]
type ml

/// Volume, liters.
[<Measure>]
type L

"""
