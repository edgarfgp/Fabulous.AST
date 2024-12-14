(**
---
title: Units of Measure
category: widgets
index: 14
---
*)

(**
# Units of Measure
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Measure("cm")

        Measure("ml", MeasurePower(LongIdent "cm", Integer "3"))

        Measure("m")

        Measure("s")

        Measure("kg")

        Measure(
            "N",
            Tuple(
                [ AppPostfix(LongIdent "kg", LongIdent "m")
                  MeasurePower(LongIdent "s", Integer "2") ],
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
