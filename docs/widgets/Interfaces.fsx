(**
---
title: Interfaces
category: widgets
index: 11
---
*)

(**
# Interfaces
Interfaces specify sets of related members that other classes implement.

For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-typedefnregularnode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/interfaces) for a comparison between the two.
*)

(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| TypeDefn(name: string, parameters: WidgetBuilder<Pattern>)                  | Creates a TypeDefnRegularNode AST node with the specified name and parameters. |
| TypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>)          | Creates a TypeDefnRegularNode AST node with the specified name and constructor. |
| TypeDefn(name: string)                        | Creates a TypeDefnRegularNode AST node with the specified name. |
*)

(**
### Modifiers

| Modifier                                   | Description                                                                                     |
|--------------------------------------------|-------------------------------------------------------------------------------------------------|
| toRecursive() | Adds a scalar indicating that the type definition is recursive.                                         |
| toPrivate()   | Sets the accessibility to the type definition to private.                                               |
| toPublic()    | Sets the accessibility of the type definition to public.                                                |
| toInternal()  | Sets the accessibility of the type definition to internal.                                               |
| typeParams(typeParams: WidgetBuilder<TyparDecls>)  | Adds type parameters to the type definition.                                                         |
| xmlDocs(xmlDocs: string list) | Adds XML documentation comments to the type definition.                                                |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds multiple attributes to the type definition.                                                        |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds a single attribute to the type definition.                                                         |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {

    AnonymousModule() {
        TypeDefn("ISprintable") { AbstractMember("Print", [ ("format", Int()) ], Unit()) }
        |> _.triviaBefore(SingleLine("Interfaces specify sets of related members that other classes implement."))

        TypeDefn("INumericFSharp") { AbstractMember("Add", [ Int(); Int() ], Int()) }

        TypeDefn("INumericDotNet") { AbstractMember("Add", [ Int(); Int() ], Int(), true) }

        TypeDefn("SomeClass1", ParenPat(TuplePat([ ParameterPat("x", Int()); ParameterPat("y", Float()) ]))) {
            InterfaceWith("IPrintable") {
                Member("this.Print()", AppExpr("printfn", [ String("%d %f"); Constant("x"); Constant("y") ]))
            }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
