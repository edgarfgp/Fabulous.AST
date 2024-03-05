(**
---
title: Values
category: recipes
index: 0
---
*)

(**
# Value Bindings
*)

(**

## Usage

*)

// #r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
// #r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"

// open Fabulous.AST
// open type Fabulous.AST.Ast
//
// Value("a", "12").returnType ("string")

(**
Will output the following code:
*)

let a: string = "12"

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Value(name: string, value: string) | Creates a value binding. |
| Value(name: string, typeParams: string list, value: string) | Creates a value binding with type parameters. |
| Value(name: string, value: WidgetBuilder<Expr>) | Creates a value binding. |
| Value(name: string, typeParams: string list, value: WidgetBuilder<Expr>) | Creates a value binding with type parameters. |
| Value(name: WidgetBuilder<Pattern>, value: string) | Creates a value binding. |
| Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) | Creates a value binding. |
| Value(name: WidgetBuilder<Pattern>, typeParams: string list, value: WidgetBuilder<Expr>) | Creates a value binding with type parameters. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| returnType(returnType: string) | Sets the return type of the value binding. |
*)
