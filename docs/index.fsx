(**
---
title: Homepage
category: docs
index: 0
---

# Fabulous.AST

Welcome to the Fabulous.AST, an Abstract Syntax Tree (AST) Domain Specific Language (DSL) for F#.

Fabulous.AST uses [Fantomas](https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html) to generate F# code from AST. This means that you can use Fabulous.AST to generate F# code that is formatted according to the Fantomas style guide. It's designed to provide a simple and expressive way to represent code as a tree of nodes. This makes it easier to manipulate and analyze code programmatically.

## Plain Fantomas

Let's take a look at an AST example in Fantomas:
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"

open Fantomas.FCS.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak

let implementationSyntaxTree =
    Oak(
        [],
        [ ModuleOrNamespaceNode(
              None,
              [ BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                    None,
                    [],
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                    Range.Zero
                )
                |> ModuleDecl.TopLevelBinding ],
              Range.Zero
          ) ],
        Range.Zero
    )

CodeFormatter.FormatOakAsync(implementationSyntaxTree)
|> Async.RunSynchronously
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**

## Using Fabulous.AST

Now let's take a look at same example using Fabulous.AST:
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

let source = AnonymousModule() { Value("x", "12") }

let oak = Tree.compile source
CodeFormatter.FormatOakAsync(oak) |> Async.RunSynchronously |> printfn "%s"

// produces the following code:
(*** include-output ***)

(** 

### Escape Hatch

You can use an `Escape Hatch` to generate code that is not supported by Fabulous.AST yet by constructing the raw [Fantomas.SyntaxOak](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak.html) node.
For example, the following code: *)

open type Fabulous.AST.Ast

let topLevelBinding: ModuleDecl =
    BindingNode(
        xmlDoc = None,
        attributes = None,
        leadingKeyword = MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
        isMutable = false,
        inlineNode = None,
        // We cannot define private yet using Fabulous.AST
        accessibility = Some(SingleTextNode("private", Range.Zero)),
        functionName = Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
        genericTypeParameters = None,
        parameters = List.Empty,
        returnType = None,
        equals = SingleTextNode("=", Range.Zero),
        expr = Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
        range = Range.Zero
    )
    |> ModuleDecl.TopLevelBinding

let sourceWithEscapeHatch =
    AnonymousModule() {
        Value("a", "11")
        EscapeHatch(topLevelBinding)
    }

Tree.compile sourceWithEscapeHatch
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

// produces the following code:
(*** include-output ***)
(**

## Key takeaway

Using Fabulous.AST, you can easily create and manipulate ASTs like this one using F# functions. For example, you can add new nodes to the AST, modify existing nodes, or traverse the AST to perform analysis or transformation tasks.

Fabulous.AST is a powerful tool for anyone who works with code and wants to automate or streamline their development workflow. Whether you're a compiler writer, a code generator, or just someone who wants to write better code faster, Fabulous.AST can help you achieve your goals.
*)
