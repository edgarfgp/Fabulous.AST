(**
---
title: Oak
category: api
index: 1
---
*)

(**
# Oak
For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-oak.html).

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Oak()                              | Creates an Oak AST node |
*)

(**
### Properties

| Properties                                                                                  | Description                                                                                     |
| ------------------------------------------------------------------------------------------- |-------------------------------------------------------------------------------------------------|
| hashDirectives(values: WidgetBuilder<ParsedHashDirectiveNode> list)                         |  a list of hash directive nodes                                                                                             |
| hashDirective(value: WidgetBuilder<ParsedHashDirectiveNode>)      |   a hash directive node                    |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.Builders.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { () }
|> _.hashDirective(NoWarn(String "0044"))
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
