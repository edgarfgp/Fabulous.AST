(**
---
title: Units of Measure
category: widgets
index: 14
---
*)

(**
# Units of Measure

Units of measure let you annotate numeric types with physical units that the
compiler checks. Declare a measure with `Measure`, and attach one to a literal
with `ConstantMeasure`.

## Contents
- [Defining a Measure](#Defining-a-Measure)
- [Derived Measures](#Derived-Measures)
- [Using a Measure](#Using-a-Measure)
- [Measure Expressions](#Measure-Expressions)
- [Negative and Fractional Powers](#Negative-and-Fractional-Powers)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Defining a Measure
A bare `Measure` declares a base unit:
*)

Oak() {
    AnonymousModule() {
        Measure("cm")
        Measure("m")
        Measure("kg")
        Measure("s")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Derived Measures
Build derived measures from powers and products of existing ones with
`MeasurePowerType`, `AppPostfix` (for a product such as `kg m`), and `Tuple`:
*)

Oak() {
    AnonymousModule() {
        Measure("ml", MeasurePowerType("cm", Integer "3"))

        Measure(
            "N",
            Tuple(
                [ AppPostfix(LongIdent "kg", LongIdent "m")
                  MeasurePowerType("s", Integer "2") ],
                "/"
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Using a Measure
Annotate a numeric literal with `ConstantMeasure`:
*)

Oak() { AnonymousModule() { Value("length", ConstantExpr(ConstantMeasure("10.0", MeasureSingle("cm")))) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Measure Expressions
Literal measures can be combined: `MeasureOperator` joins two measures with an operator such as `*`,
`MeasureSeq` multiplies measures by juxtaposition (`kg m`), `MeasureDivide` divides them (with a
single argument it gives a reciprocal such as `/s`, "per second"), and `MeasureParen` groups a
sub-expression in parentheses:
*)

Oak() {
    AnonymousModule() {
        Value("torque", ConstantExpr(ConstantMeasure(Float(12.5), MeasureOperator("*", "N", "m"))))

        Value("frequency", ConstantExpr(ConstantMeasure(Float(50.0), MeasureDivide("s"))))

        Value(
            "force",
            ConstantExpr(
                ConstantMeasure(
                    Float(9.81),
                    MeasureDivide(
                        MeasureSeq([ MeasureSingle("kg"); MeasureSingle("m") ]),
                        MeasurePower("s", Integer(2))
                    )
                )
            )
        )

        Value(
            "gravity",
            ConstantExpr(ConstantMeasure(Float(9.81), MeasureDivide("m", MeasureParen(MeasureOperator("*", "s", "s")))))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Negative and Fractional Powers
`Negate` makes an exponent negative and `Rational` writes a fractional exponent such as `(1/2)`.
Both work in measure literals (`MeasurePower`) and in measure types (`MeasurePowerType`):
*)

Oak() {
    AnonymousModule() {
        Measure("Hz", MeasurePowerType("s", Negate(Integer(1))))

        Value("frequency", ConstantExpr(ConstantMeasure(Float(50.0), MeasurePower("s", Negate(Integer(1))))))

        Value(
            "noiseDensity",
            ConstantExpr(ConstantMeasure(Float(0.5), MeasurePower("Hz", Rational("1", "/", "2")))),
            AppPrefix(Float(), MeasurePowerType("Hz", Rational("1", "/", "2")))
        )

        Value(
            "rate",
            ConstantExpr(ConstantMeasure(Float(2.0), MeasurePower("Hz", Negate(Rational("1", "/", "2"))))),
            AppPrefix(Float(), MeasurePowerType("Hz", Negate(Rational("1", "/", "2"))))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
