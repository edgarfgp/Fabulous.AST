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
open Fantomas.FCS.Text

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

(**
## Using EscapeHatch for raw SyntaxOak nodes

Example: injecting a raw top-level binding node into an anonymous module.
*)

open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        // Use the DSL normally
        Value("x", Int 42)

        // If you must inject a raw SyntaxOak node, wrap it in EscapeHatch
        EscapeHatch(
            ModuleDecl.TopLevelBinding(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("y", Range.Zero)) ], Range.Zero)),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                    Range.Zero
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Likewise, if you have other raw nodes (e.g., `TypeDefn.Abbrev`, `EnumCaseNode`, etc.), inject them via `EscapeHatch(...)`.
The preferred approach remains to use the provided widgets (`Abbrev`, `EnumCase`, `Record`, `Value`, ...), which do not require `EscapeHatch`.
*)
