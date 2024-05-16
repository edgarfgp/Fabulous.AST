﻿(**
---
title: Unions
category: widgets
index: 6
---
*)

(**
# Discriminated Unions
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Union(name: string) | Creates a new union with the specified name. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| members() | Sets the members of the union. |
| xmlDocs(xmlDocs: string list) | Adds XML documentation to the union. |
| typeParams(typeParams: string list) | Adds type parameters to the union. |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds attributes to the union. |
| attributes(attributes: string list) | Adds attributes to the union. |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds an attribute to the union. |
| attribute(attribute: string) | Adds an attribute to the union. |
| toPrivate() | Makes the value binding private. |
| toInternal() | Makes the value binding internal. |
| toPublic() | Makes the value binding public. |
*)

(**

## Usage

*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        (Union("Colors") {
            UnionCase("Red")
            UnionCase("Green")
            UnionCase("Blue")
            UnionCase("Yellow")
        })
            .attribute(Attribute "AutoOpen")
            .xmlDocs([ "This is a union type" ])
            .toPrivate()
            .members () {
            Property("CurrentColor", ConstantExpr(String "Red")).toStatic ()

            Method(
                "this.GetColor",
                ParameterPat("c", "Colors"),
                MatchExpr(
                    "c",
                    [ MatchClauseExpr("Red", String "Red")
                      MatchClauseExpr(NamedPat("Green"), String "Green")
                      MatchClauseExpr("Blue", String "Blue")
                      MatchClauseExpr(NamedPat("Yellow"), ConstantExpr(String "Yellow")) ]
                )

            )
        }

        (Union("Colors2") {
            UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

            UnionCase("Green")
            UnionCase("Blue")
            UnionCase("Yellow")
        })
            .typeParams(PostfixList([ "'other" ]))
            .members () {
            Property("Name", ConstantExpr("name")).toStatic ()
        }
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

/// This is a union type
[<AutoOpen>]
type Colors =
    private
    | Red
    | Green
    | Blue
    | Yellow

    static member CurrentColor = "Red"

    member this.GetColor(c: Colors) =
        match c with
        | Red -> "Red"
        | Green -> "Green"
        | Blue -> "Blue"
        | Yellow -> "Yellow"

type Colors2<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    static member Name = "name"
