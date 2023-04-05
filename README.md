# Fabulous.AST
[![build](https://img.shields.io/github/actions/workflow/status/edgarfgp/Fabulous.AST/build.yml?branch=main)](https://github.com/edgarfgp/Fabulous.AST/actions/workflows/build.yml) [![NuGet version](https://img.shields.io/nuget/v/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.Avalonia) [![NuGet downloads](https://img.shields.io/nuget/dt/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.AST)

Welcome to the Fabulous.AST, an Abstract Syntax Tree (AST) Domain Specific Language (DSL) for F#.

Fabulous.AST uses [Fantomas](https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html) to generate F# code from AST. This means that you can use Fabulous.AST to generate F# code that is formatted according to the Fantomas style guide. It's designed to provide a simple and expressive way to represent code as a tree of nodes. This makes it easier to manipulate and analyze code programmatically.

Let's take a look at an AST example in Fantomas:

```fsharp
open FSharp.Compiler.Text
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

open Fantomas.Core

CodeFormatter.FormatOakAsync(implementationSyntaxTree)
|> Async.RunSynchronously
|> printfn "%s"
```
produces the following code:

```fsharp
let x = 12
```

Now let's take a look at same example using Fabulous.AST:

```fsharp
open type Fabulous.AST.Ast

let source = 
    AnonymousModule() { 
        Value("x", "12")
    }

open Fantomas.Core

let oak = Tree.compile source
let res = CodeFormatter.FormatOakAsync(oak) |> Async.RunSynchronously
printfn $"%s{res}"
```
produces the following code:

```fsharp
let x = 12
```

You can use an `Escape Hatch` to generate code that is not supported by Fabulous.AST yet. For example, the following code:

```fsharp
open type Fabulous.AST.Ast

let source = 
    AnonymousModule() { 
        Value("a", "11")
        BindingNode(
            None,
            None,
            MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
            false,
            None,
            None,
            Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
            None,
            List.Empty,
            None,
            SingleTextNode("=", Range.Zero),
            Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
            Range.Zero
        )
    }

open Fantomas.Core

let oak = Tree.compile source

let oak = Tree.compile source
let res = CodeFormatter.FormatOakAsync(oak) |> Async.RunSynchronously
printfn $"%s{res}"
```
produces the following code:

```fsharp
let a = 11
let x = 12
```

Using Fabulous.AST, you can easily create and manipulate ASTs like this one using F# functions. For example, you can add new nodes to the AST, modify existing nodes, or traverse the AST to perform analysis or transformation tasks.

Fabulous.AST is a powerful tool for anyone who works with code and wants to automate or streamline their development workflow. Whether you're a compiler writer, a code generator, or just someone who wants to write better code faster, Fabulous.AST can help you achieve your goals.

### WIP widgets

#### Namespaces and modules

| Widget          | Description         | F# code             |
|-----------------|---------------------|---------------------|
| AnonymousModule | An anonymous module | ```()```            |
| Namespace       | A namespace         | ```namespace ABC``` |
| Module          | A module            | ```module ABC```    |
| NestedModule    | A module            | ```module ABC =```  |

#### Open directives

| Widget   | Description         | F# code                 |
|----------|---------------------|-------------------------|
| Open     | An open directive   | ```open ABC```          |
| OpenType | An open directive   | ```open type ABC.XYZ``` |

#### Let bindings

| Widget   | Description           | F# code                         |
|----------|-----------------------|---------------------------------|
| Value    | A let binding         | ```let x = 12```                |
| Literal  | A literal definition  | ```[<Literal>] let x = 12```    |
| Function | A function definition | ```let f x = x + 1```           |

#### Type definitions

| Widget    | Description       | F# code                            |
|-----------|-------------------|------------------------------------|
| Alias     | A type definition | ```type MyInt = int```             |
| Record    | A record type     | ```type R = { x: int; y: int }```  |
| Union     | A union type      | ```type U = A \| B of int```       |
| Class     | A class type      | ```type MyClass() = class end```   |
| Struct    | A struct type     | ```type MyStruct() = struct end``` |
| Interface | An interface type | ```type I = interface end```       |

#### Control flow definitions

| Widget     | Description        | F# code                       |
|------------|--------------------|-------------------------------|
| If         | An if expression   | ```if x = 12 then 1```        |
| IfElse     | An if expression   | ```if x = 12 then 1 else 2``` |
| Match      | A match expression | ```match x with```            |
| For        | A for loop         | ```for i in 1..10 do```       |
| While      | A while loop       | ```while x < 10 do```         |
