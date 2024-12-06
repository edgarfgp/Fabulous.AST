(**
---
title: Discriminated Unions
category: widgets
index: 12
---
*)

(**
# Discriminated Unions
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Option") {
            UnionCase("Some", "'a")
            UnionCase("None")
        }
        |> _.typeParams(PostfixList("'a"))

        Union("Shape") {
            UnionCase("Line")
            UnionCase("Rectangle", [ Field("width", Float()); Field("length", Float()) ])
            UnionCase("Circle", Field("radius", Float()))
            UnionCase("Prism", [ Field("width", Float()); Field("float"); Field("height", Float()) ])
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
