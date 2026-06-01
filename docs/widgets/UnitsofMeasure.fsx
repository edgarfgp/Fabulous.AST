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
- [Defining a Measure](#defining-a-measure)
- [Derived Measures](#derived-measures)
- [Using a Measure](#using-a-measure)
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
`MeasurePowerType`, `AppPrefix`, and `Tuple`:
*)

Oak() {
    AnonymousModule() {
        Measure("ml", MeasurePowerType("cm", Integer "3"))

        Measure("N", Tuple([ AppPrefix(LongIdent "kg", LongIdent "m"); MeasurePowerType("s", Integer "2") ], "/"))
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
