(**
---
title: Oak
category: widgets
index: 1
---
*)

(**
# Oak
It is the entry point for out DSL, it can contain one or more `Namespace`, `Module` and `AnonymousModule` nodes. `Module` must be inside a `AnonymousModule` or `Namespace` nodes.

For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-oak.html).

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Oak()                              | Creates an Oak AST node |
*)

(**
### Modifiers

| Modifiers                                                                                  | Description                                                                                     |
| ------------------------------------------------------------------------------------------- |-------------------------------------------------------------------------------------------------|
| hashDirectives(values: WidgetBuilder<ParsedHashDirectiveNode> list)                         |  a list of hash directive nodes                                                                                             |
| hashDirective(value: WidgetBuilder<ParsedHashDirectiveNode>)      |   a hash directive node                    |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() { NoWarn(String "0044") }

    Namespace("Widgets") { Module("WidgetsModule") { Value("x", String("12")) } }

    Namespace("Widgets.WidgetModule") {
        Function("widgetFunction", [ ParameterPat("x"); ParameterPat("y") ], String("12"))
    }
    |> _.toImplicit()
    |> _.triviaBefore(Newline())

    AnonymousModule() { Module("WidgetsModule") { Value("y", String("12")) } }
    |> _.triviaBefore(Newline())

    AnonymousModule() { Value("y", String("12")) } |> _.triviaBefore(Newline())
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
