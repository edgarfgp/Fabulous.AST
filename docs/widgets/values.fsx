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
| Value(name: string, value: string, ?hasQuotes: bool) | Creates a value binding with name and value, and optionally specifies if the value should be quoted. |
| Value(name: string, value: WidgetBuilder<Expr>) | Creates a value binding with name and value expression. |
| Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) | Creates a value binding with name pattern and value expression. |
| Value(name: WidgetBuilder<Pattern>, value: string, ?hasQuotes: bool) | Creates a value binding with name pattern and value expression, and optionally specifies if the value should be quoted. |
| Value(name: string, typeParams: string list, value: WidgetBuilder<Expr>) | Creates a value binding with name, type parameters and value expression. |
| Value(name: string, typeParams: string list, value: string, ?hasQuotes: bool) | Creates a value binding with name, type parameters and value, and optionally specifies if the value should be quoted. |
| Value(name: WidgetBuilder<Pattern>, typeParams: string list, value: WidgetBuilder<Expr>) | Creates a value binding with name pattern, type parameters and value expression. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: string list) | Adds XML documentation to the value binding. |
| attributes(this: WidgetBuilder<BindingNode>) | Adds attributes of the value binding. |
| attributes(this: WidgetBuilder<BindingNode>, attributes: string list) | Adds attributes to the value binding. |
| attribute(this: WidgetBuilder<BindingNode>, attribute: WidgetBuilder<AttributeNode>) | Adds an attribute to the value binding. |
| attribute(this: WidgetBuilder<BindingNode>, attribute: string) | Adds an attribute to the value binding. |
| toPrivate(this: WidgetBuilder<BindingNode>) | Makes the value binding private. |
| toInternal(this: WidgetBuilder<BindingNode>) | Makes the value binding internal. |
| toPublic(this: WidgetBuilder<BindingNode>) | Makes the value binding public. |
| returnType(this: WidgetBuilder<BindingNode>, returnType: WidgetBuilder<Type>) | Sets the return type of the value binding. |
| returnType(this: WidgetBuilder<BindingNode>, returnType: string) | Sets the return type of the value binding. |
| toMutable(this: WidgetBuilder<BindingNode>) | Makes the value binding mutable. |
| toInlined(this: WidgetBuilder<BindingNode>) | Makes the value binding inlined. |
| toStatic(this: WidgetBuilder<BindingNode>) | Makes the value binding static. |
| typeParameters(this: WidgetBuilder<BindingNode>, typeParams: string list) | Adds type parameters to the value binding. |
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

AnonymousModule() {
    Value("value", "12")

    Value("value1", "12", hasQuotes = false)

    Value(
        "value3",
        IfThenElseExpr(
            InfixAppExpr(ConstantExpr("0", false), "=", ConstantExpr(Constant("12", false))),
            ConstantExpr(ConstantUnit()),
            ConstantExpr(ConstantUnit())
        )
    )

    Value(
        NamedPat("value4"),
        IfThenElseExpr(
            InfixAppExpr(ConstantExpr("0", false), "=", ConstantExpr(Constant("12", false))),
            ConstantExpr(ConstantUnit()),
            ConstantExpr(ConstantUnit())
        )
    )

    Value(NamedPat("value5"), "12")

    Value(NamedPat("value6"), "12", false)

    Value("value7", [ "'a" ], "12")

    Value("value8", [ "'a" ], ConstantExpr("12"))

    Value(NamedPat("value9"), [ "'a" ], ConstantExpr("12"))

    Value("value10", "12", false).returnType (Int32())

    Value("value11", "12", false).returnType(Int32()).toPrivate ()

    Value("value12", "12").attribute ("Literal")

    Value("value13", "12", false).returnType(Int32()).toMutable ()

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
