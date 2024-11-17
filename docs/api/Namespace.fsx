(**
---
title: Namespace
category: api
index: 2
---
*)

(**
# Namespace
For details on how the AST node works, please refer to the [Fantomas documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-moduleornamespacenode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/namespaces) for a comparison between the two.

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Namespace(name: string)                  | Creates a NamespaceNode AST node with the specified name. |
| ImplicitNamespace(name: string)          | Creates an ImplicitNamespaceNode AST node with the specified name. |
| AnonymousModule()                        | Creates an AnonymousModuleNode AST node. |
*)

(**
### Properties

| Properties                                   | Description                                                                                     |
|---------------------------------------------|-------------------------------------------------------------------------------------------------|
| toRecursive(this: WidgetBuilder<TopLevelModuleNode>) | Adds a scalar indicating that the module is recursive.                                         |
| toPrivate(this: WidgetBuilder<TopLevelModuleNode>)   | Sets the accessibility of the module to private.                                               |
| toPublic(this: WidgetBuilder<TopLevelModuleNode>)    | Sets the accessibility of the module to public.                                                |
| toInternal(this: WidgetBuilder<TopLevelModuleNode>)  | Sets the accessibility of the module to internal.                                              |
| xmlDocs(this: WidgetBuilder<TopLevelModuleNode>, xmlDocs: string list) | Adds XML documentation comments to the module.                                                |
| attributes(this: WidgetBuilder<TopLevelModuleNode>, attributes: WidgetBuilder<AttributeNode> list) | Adds multiple attributes to the module.                                                        |
| attribute(this: WidgetBuilder<TopLevelModuleNode>, attribute: WidgetBuilder<AttributeNode>) | Adds a single attribute to the module.                                                         |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.Builders.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    Namespace("Widgets") {
        TypeDefn("MyWidget1") { Property("this.WidgetName", String("Widget1")) }

        Module("WidgetsModule") { Value("widgetName", String("Widget2")) }
    }
    |> _.triviaBefore(SingleLine("Namespace"))
    |> _.triviaAfter(Newline())

    ImplicitNamespace("Implicit.Namespace1") { () }
    |> _.triviaBefore(SingleLine("Implicit Namespace"))
    |> _.triviaAfter(Newline())

    AnonymousModule() { Value("x", ConstantExpr(Int(3))) }
    |> _.triviaBefore(SingleLine("Anonymous Module"))
    |> _.triviaAfter(Newline())

    Namespace("Outer") {
        Class("MyClass") {
            Method("this.X", ParenPat(ParameterPat(ConstantPat(Constant "x"))), InfixAppExpr("p", "+", Int(1)))
        }
        |> _.triviaBefore(SingleLine("Full name: Outer.MyClass"))
    }
    |> _.triviaBefore(SingleLine("Nested Namespaces"))
    |> _.triviaAfter(Newline())

    Namespace("Outer.Inner") {
        Class("MyClass") { Property("this.Prop1", String("X")) }
        |> _.triviaBefore(SingleLine("Full name: Outer.Inner.MyClass"))
    }

}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
