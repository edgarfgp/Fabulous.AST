(**
---
title: Classes
category: widgets
index: 6
---
*)

(**
# Classes
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Class(name: string) | Creates a new class with the specified name. |
| Class(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) | Creates a new class with the specified name and constructor. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| xmlDocs(xmlDocs: string list) | Adds XML documentation to the class. |
| typeParams(typeParams: string list) | Adds type parameters to the class. |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds attributes to the class. |
| attributes(attributes: string list) | Adds attributes to the class. |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds an attribute to the class. |
| attribute(attribute: string) | Adds an attribute to the class. |
| toPrivate() | Makes the class private. |
| toInternal() | Makes the class internal. |
| toPublic() | Makes the class public. |
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
        Class("Person") { Property("this.Name", String("")) }

        Class("Person2", ImplicitConstructor(ParenPat(TuplePat([ ParameterPat("name") ])))) {
            Property("this.Name", ConstantExpr("name"))
        }

        Class(
            "Person3",
            ImplicitConstructor(ParenPat(TuplePat([ ParameterPat("name", String()); ParameterPat("?age", Int()) ])))
        ) {
            Property("this.Name", ConstantExpr("name"))
        }

        (Class("Person4", ImplicitConstructor(ParenPat(ParameterPat("name", String())))) {
            Property("this.Name", ConstantExpr("name"))
        })
            .attribute (Attribute("Struct"))

        (Class("Person5") { Property("this.Name", ConstantExpr("")) })
            .typeParams ([ "'a"; "'b" ])
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

type Person() =
    member this.Name = ""

type Person2(name) =
    member this.Name = name

type Person3(name: string, ?age: int) =
    member this.Name = name

[<Struct>]
type Person4(name: string) =
    member this.Name = name

type Person5<'a, 'b>() =
    member this.Name = ""
