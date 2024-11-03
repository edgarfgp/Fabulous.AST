(**
---
title: TopLevelModule
category: api
index: 2
---
*)

(**
# TopLevelModule
For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-oak.html).

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| TopLevelModule(name: string)                  | Creates a TopLevelModule AST node with the specified name. |
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

Oak() { TopLevelModule("MyModule") { () } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
