(**
---
title: Discriminated Unions
category: widgets
index: 12
---
*)

(**
# Discriminated Unions

Classes are types that represent objects that can have properties, methods, and events.
Discriminated unions provide support for values that can be one of a number of named cases, possibly each with different values and types.
Discriminated unions are useful for heterogeneous data; data that can have special cases, including valid and error cases; data that varies in type from one instance to another; and as an alternative for small object hierarchies.
In addition, recursive discriminated unions are used to represent tree data structures.

For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-typedefnunionnode.html).
*)

(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Union(name: string)                 | Creates a TypeDefnUnionNode AST node with the specified name. |
*)

(**
### Modifiers

| Modifier                                   | Description                                                                                     |
|--------------------------------------------|-------------------------------------------------------------------------------------------------|
| members() | Adds members to the type definition.                                                         |
| typeParams(typeParams: WidgetBuilder<TyparDecls>)  | Adds type parameters to the type definition.                                                         |
| xmlDocs(xmlDocs: string list) | Adds XML documentation comments to the type definition.                                                |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds multiple attributes to the type definition.                                                        |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds a single attribute to the type definition.                                                         |
| toRecursive() | Adds a scalar indicating that the type definition is recursive.                                         |
| toPrivate()   | Sets the accessibility to the type definition to private.                                               |
| toPublic()    | Sets the accessibility of the type definition to public.                                                |
| toInternal()  | Sets the accessibility of the type definition to internal.                                               |
| xmlDocs(xmlDocs: string list) | Adds XML documentation comments to the type definition.                                                |
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
