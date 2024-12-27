# Fabulous.AST
[![build](https://img.shields.io/github/actions/workflow/status/edgarfgp/Fabulous.AST/build.yml?branch=main)](https://github.com/edgarfgp/Fabulous.AST/actions/workflows/build.yml) [![NuGet version](https://img.shields.io/nuget/v/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.AST) [![NuGet downloads](https://img.shields.io/nuget/dt/Fabulous.AST)](https://www.nuget.org/packages/Fabulous.AST)

Welcome to Fabulous.AST, an Abstract Syntax Tree (AST) Domain Specific Language (DSL) for F#.

Fabulous.AST leverages [Fantomas](https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html) to generate F# code from AST. This allows you to create F# code that adheres to the Fantomas style guide while providing a simple and expressive way to represent code as a tree of nodes. This approach simplifies programmatic code manipulation and analysis.

## Features

- Simplified AST creation for F# code
- Integration with Fantomas for consistent code formatting
- Expressive DSL for representing code structures
- Easy-to-use API for code generation and manipulation

## Installation

Install Fabulous.AST via NuGet:

```
dotnet add package Fabulous.AST
```

## Usage

Let's compare AST creation using Fantomas directly and Fabulous.AST:

### Fantomas AST Example

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

### Fabulous.AST Example

```fsharp
open Fantomas.Core
open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { 
    AnonymousModule() { 
        Value("x", "12") 
    }
}
|> Gen.mkOak
|> Gen.run
```

Both examples produce the following F# code:

```fsharp
let x = 12
```

As you can see, Fabulous.AST significantly simplifies the process of creating ASTs for F# code.

## Documentation

For comprehensive information about Fabulous.AST, visit our [documentation site](https://edgarfgp.github.io/Fabulous.AST/).

Additional resources:
- [API Reference](https://edgarfgp.github.io/Fabulous.AST/reference/index.html)
- [Contributor Guide](CONTRIBUTING.md)

## Supporting Fabulous.AST

Show your support for Fabulous.AST:
1. Star this repository and the [Fabulous.AST project](https://github.com/edgarfgp/Fabulous.AST) on GitHub.
2. Become a sponsor through the GitHub Sponsors program.

## Contributing

We welcome contributions to Fabulous.AST!

- Found a bug or have a suggestion? Open an issue, and we'll look into it promptly.
- Want to contribute code? PRs are always welcome! Please create them from the `main` branch and follow our [Contributor Guide](CONTRIBUTING.md).

## License

Fabulous.AST is released under the [MIT License](LICENSE.md).
