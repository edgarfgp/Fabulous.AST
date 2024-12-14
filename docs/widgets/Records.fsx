(**
---
title: Records
category: widgets
index: 13
---
*)

(**
# Records
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Record("Point") {
            Field("X", Float())
            Field("Y", Float())
            Field("Z", Float())
        }

        Record("StructPoint") {
            Field("X", Float())
            Field("Y", Float())
            Field("Z", Float())
        }
        |> _.attribute(Attribute "Struct")

        Record("Person") { Field("Name", LongIdent("'other")) }
        |> _.typeParams(PostfixList([ "'other" ]))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
