(**
---
title: Functions
category: widgets
index: 3
---
*)

(**
# Function Bindings
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Function(name: string, parameters: WidgetBuilder<Pattern>, value: StringVariant) | Creates a function binding with name, parameters and value expression. |
| Function(name: string, parameters: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) | Creates a function binding with name, parameters and value expression. |
| Function(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) | Creates a function binding with name, type parameters, parameters and value expression. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| xmlDocs(xmlDocs: string list) | Adds XML documentation to the value binding. |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds attributes of the value binding. |
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
        Function("x", ParameterPat("i"), ConstantExpr(ConstantUnit()))
        Function("x1", NamedPat("i"), ConstantExpr(ConstantUnit()))
        Function("x2", ParenPat(NamedPat("i")), ConstantExpr(ConstantUnit()))
        Function("x3", ParenPat(ParameterPat(NamedPat("i"), Int())), ConstantExpr(ConstantUnit()))

        Function("x4", TuplePat([ NamedPat("i"); NamedPat("j"); NamedPat("k") ]), ConstantExpr(ConstantUnit()))

        Function("x5", [ NamedPat("i"); NamedPat("j"); NamedPat("k") ], ConstantExpr(ConstantUnit()))

        Function(
            "x6",
            TuplePat(
                [ ParameterPat(NamedPat("i"), Int())
                  ParameterPat(NamedPat("j"), String())
                  ParameterPat(NamedPat("k"), Boolean()) ]
            ),
            ConstantExpr(ConstantUnit())
        )

        Open("System")

        Function("x7", NamedPat("i"), ConstantExpr(ConstantUnit()))
            .attribute (Attribute("Obsolete", ParenExpr(ConstantExpr("Use bar instead"))))

        Function("x8", NamedPat("i"), ConstantExpr(ConstantUnit())).xmlDocs [ "Im a function" ]

        Function("x9", NamedPat("i"), ConstantExpr(ConstantUnit())).returnType (Unit())

        Function(
            "foo",
            TuplePat([ ParameterPat("x", "'T"); ParameterPat(NamedPat("i"), "'U") ]),
            ConstantExpr(ConstantUnit())
        )
            .returnType (Unit())

        Function("x10", NamedPat("i"), ConstantExpr(ConstantUnit())).toInlined ()

        Function("x11", NamedPat("i"), ConstantExpr(ConstantUnit())).toPublic ()

        Function("y", NamedPat("i"), ConstantExpr(ConstantUnit())).toPrivate ()

        Function("z", NamedPat("i"), ConstantExpr(ConstantUnit())).toInternal ()
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

let x i = ()
let x1 i = ()
let x2 (i) = ()
let x3 (i: int) = ()
let x4 (i, j, k) = ()
let x5 i j k = ()
let x6 (i: int, j: string, k: bool) = ()

open System

[<Obsolete("Use bar instead")>]
let x7 i = ()

/// I'm a function
let x8 i = ()
let x9 i : unit = ()
let foo (x: 'T, i: 'U) : unit = ()
let inline x10 i = ()
let public x11 i = ()
let private y i = ()
let internal z i = ()
