(**
---
title: Oak
category: widgets
index: 1
---
*)

(**
# Oak

## Overview
The Oak widget is the root container for all F# AST nodes in the Fabulous.AST DSL. It serves as the entry point for creating F# code and must be used as the outermost container for your AST structure.

## Basic Usage
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { AnonymousModule() { Value("x", Int(42)) } }
|> Gen.mkOak // Convert to an Oak AST node
|> Gen.run // Generate the F# code string
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Structure
The Oak widget can contain any combination of the following top-level nodes:

- `AnonymousModule`: For file-level code
- `Namespace`: For organizing code in namespaces
- `HashDirective` : For compiler directives

> Note: `Module` nodes must be nested inside either `AnonymousModule` or `Namespace` nodes.

## Adding Hash Directives
Add compiler directives at the Oak level:
*)

Oak() { AnonymousModule() { NoWarn("0044") } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Example with Multiple Components
Here's a more complete example showing how Oak can contain multiple components:
*)

Oak() {
    // Hash directive at the file level
    AnonymousModule() { NoWarn("0044") }

    // A namespace with a module
    Namespace("Widgets") { Module("WidgetModule") { Value("x", String("12")) } }

    // An anonymous module with code
    AnonymousModule() { Value("y", String("12")) } |> _.triviaBefore(Newline())
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
