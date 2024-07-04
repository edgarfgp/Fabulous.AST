# Fabulous.AST
[![build](https://img.shields.io/github/actions/workflow/status/edgarfgp/Fabulous.AST/build.yml?branch=main)](https://github.com/edgarfgp/Fabulous.AST/actions/workflows/build.yml) [![NuGet version](https://img.shields.io/nuget/v/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.AST) [![NuGet downloads](https://img.shields.io/nuget/dt/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.AST)

Welcome to the Fabulous.AST, an Abstract Syntax Tree (AST) Domain Specific Language (DSL) for F#.

Fabulous.AST uses [Fantomas](https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html) to generate F# code from AST. This means that you can use Fabulous.AST to generate F# code that is formatted according to the Fantomas style guide. It's designed to provide a simple and expressive way to represent code as a tree of nodes. This makes it easier to manipulate and analyze code programmatically.

Let's take a look at an AST example in Fantomas:

```fsharp
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
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
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"
```
produces the following code:

```fsharp
let x = 12
```

Now let's take a look at same example using Fabulous.AST:

```fsharp
open Fantomas.Core
open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { 
    AnonymousModule() { 
        Value("y", "12") 
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"
```
produces the following code:

```fsharp
let x = 12
```

## Documentation

The full documentation for Fabulous.AST can be found at [Fabulous.AST](https://edgarfgp.github.io/Fabulous.AST/).

Other useful links:
- [API Reference](https://edgarfgp.github.io/Fabulous.AST/reference/index.html)
- [Contributor Guide](CONTRIBUTING.md)

## Supporting Fabulous.AST

The simplest way to show us your support is by giving this project and the [Fabulous.AST project](https://github.com/edgarfgp/Fabulous.AST) a star.
You can also support us by becoming our sponsor on the GitHub Sponsors program.

## Contributing

Have you found a bug or have a suggestion of how to enhance Fabulous.AST? Open an issue, and we will take a look at it as soon as possible.

Do you want to contribute with a PR? PRs are always welcome, just make sure to create it from the correct branch (main) and follow the [Contributor Guide](CONTRIBUTING.md).
