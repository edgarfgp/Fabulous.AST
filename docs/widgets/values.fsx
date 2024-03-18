(**
---
title: Values
category: widgets
index: 2
---
*)

(**
# Value Bindings
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Value(name: string, value: string) | Creates a value binding with name and value, and optionally specifies if the value should be quoted. |
| Value(name: string, value: WidgetBuilder<Expr>) | Creates a value binding with name and value expression. |
| Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) | Creates a value binding with name pattern and value expression. |
| Value(name: WidgetBuilder<Pattern>, value: string) | Creates a value binding with name pattern and value expression, and optionally specifies if the value should be quoted. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| xmlDocs(, xmlDocs: string list) | Adds XML documentation to the value binding. |
| attributes() | Adds attributes of the value binding. |
| attributes(attributes: string list) | Adds attributes to the value binding. |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds an attribute to the value binding. |
| attribute(attribute: string) | Adds an attribute to the value binding. |
| toPrivate() | Makes the value binding private. |
| toInternal() | Makes the value binding internal. |
| toPublic() | Makes the value binding public. |
| returnType(returnType: WidgetBuilder<Type>) | Sets the return type of the value binding. |
| returnType(returnType: string) | Sets the return type of the value binding. |
| toMutable() | Makes the value binding mutable. |
| toInlined() | Makes the value binding inlined. |
| toStatic() | Makes the value binding static. |
| hasQuotes() | Specifies if the value should be quoted. |
| typeParameters(typeParams: string list) | Adds type parameters to the value binding. |
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
        Value("value", Quoted "12")

        Value("value1", Unquoted "12")

        Value(
            "value3",
            IfThenElseExpr(
                InfixAppExpr(ConstantExpr(Unquoted "0"), "=", ConstantExpr(Constant(Unquoted "12"))),
                ConstantExpr(ConstantUnit()),
                ConstantExpr(ConstantUnit())
            )
        )

        Value(
            NamedPat("value4"),
            IfThenElseExpr(
                InfixAppExpr(ConstantExpr(Unquoted "0"), "=", ConstantExpr(Constant(Unquoted "12"))),
                ConstantExpr(ConstantUnit()),
                ConstantExpr(ConstantUnit())
            )
        )

        Value(NamedPat("value5"), Quoted "12")

        Value(NamedPat("value6"), Unquoted "12")

        Value("value7", Quoted "12").typeParams ([ "'a" ])

        Value("value8", ConstantExpr(Quoted "12")).typeParams ([ "'a" ])

        Value(NamedPat("value9"), ConstantExpr(Quoted "12")).typeParams ([ "'a" ])

        Value("value10", Unquoted "12").returnType (Int32())

        Value("value11", Unquoted "12").returnType(Int32()).toPrivate ()

        Value("value12", Quoted "12").attribute ("Literal")

        Value("value13", Unquoted "12").returnType(Int32()).toMutable ()

    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

let value = "12"
let value1 = 12
let value3 = if 0 = 12 then () else ()
let value4 = if 0 = 12 then () else ()
let value5 = "12"
let value6 = 12
let value7<'a> = "12"
let value8<'a> = "12"
let value9<'a> = "12"
let value10: int = 12
let private value11: int = 12

[<Literal>]
let value12 = "12"

let mutable value13: int = 12
