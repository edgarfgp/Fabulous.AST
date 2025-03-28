(**
---
title: Homepage
category: docs
index: 0
---

# Fabulous.AST

## Contents
- [What is AST?](#what-is-ast)
- [Why Use AST for Code Generation?](#why-use-ast-for-code-generation)
- [Code Generation Approaches](#code-generation-approaches)
  - [String Interpolation](#string-interpolation)
  - [Compiler AST](#compiler-ast)
  - [Fantomas Oak AST](#fantomas-oak-ast)
  - [Fabulous.AST DSL](#fabulousast-dsl)
- [Getting Started](#getting-started)
- [Documentation Structure](#documentation-structure)

## What is AST?

`AST` stands for Abstract Syntax Tree. It is a tree representation of the abstract syntactic structure of source code written in a programming language.
It is used by compilers to analyze, transform, and generate code.

## Why Use AST for Code Generation?

You can generate code by just using strings and string interpolation. But there are several reasons why you should not do that:

- It's error-prone and hard to maintain.
- If the code you are generating is complex, then it's even harder to generate it using string interpolation.
- You will have to write extra code to handle edge cases, ensure the generated code is valid, and manage formatting and indentation.

ASTs offer several benefits for code generation:

- The code generated is compliant with the F# syntax and will most likely be valid and compilable.
- It provides a more structured way to generate code.
- It allows you to manipulate the code programmatically.
- It enables you to generate more complex code in a more maintainable way.

## Code Generation Approaches

### String Interpolation

This is a simple example of generating code using `StringBuilder` and string interpolation:
*)

open System.Text
let code = StringBuilder()
code.AppendLine("module MyModule =")
code.AppendLine("    let x = 12")
code |> string |> printfn "%s"

// produces the following code:
(*** include-output ***)

(**

> Quote from fantomas: For mercy's sake don't use string concatenation when generating F# code, use Fantomas instead. It is battle tested and proven technology!

### Compiler AST

The official F# compiler AST:

- It is very verbose and hard to read.
- It is difficult to manipulate or analyze programmatically.
- Contains a lot of information that is not relevant to the code itself.

You can see a live example using [Fantomas tools](https://fsprojects.github.io/fantomas-tools/#/ast?data=N4KABGBEAmCmBmBLAdrAzpAXFSAacUiaAYmolmPAIYA2as%2BEkAxgPZwWQ2wAuYAHmAC8YAIwAmPAUix%2BAByrJoFHgCcArrBABfIA)
*)

open Fantomas.FCS.Syntax
open Fantomas.FCS.SyntaxTrivia
open Fantomas.FCS.Text
open Fantomas.FCS.Xml

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"

ParsedInput.ImplFile(
    ParsedImplFileInput.ParsedImplFileInput(
        fileName = "tmp.fsx",
        isScript = true,
        qualifiedNameOfFile = QualifiedNameOfFile.QualifiedNameOfFile(Ident("Tmp$fsx", Range.Zero)),
        scopedPragmas = [],
        hashDirectives = [],
        contents =
            [ SynModuleOrNamespace.SynModuleOrNamespace(
                  longId = [ Ident("Tmp", Range.Zero) ],
                  isRecursive = false,
                  kind = SynModuleOrNamespaceKind.AnonModule,
                  decls =
                      [ SynModuleDecl.Let(
                            isRecursive = false,
                            bindings =
                                [ SynBinding.SynBinding(
                                      accessibility = None,
                                      kind = SynBindingKind.Normal,
                                      isInline = false,
                                      isMutable = false,
                                      attributes = [],
                                      xmlDoc = PreXmlDoc.Empty,
                                      valData =
                                          SynValData.SynValData(
                                              memberFlags = None,
                                              valInfo =
                                                  SynValInfo.SynValInfo(
                                                      curriedArgInfos = [],
                                                      returnInfo =
                                                          SynArgInfo.SynArgInfo(
                                                              attributes = [],
                                                              optional = false,
                                                              ident = None
                                                          )
                                                  ),
                                              thisIdOpt = None
                                          ),
                                      headPat =
                                          SynPat.Named(
                                              ident = SynIdent.SynIdent(ident = Ident("x", Range.Zero), trivia = None),
                                              isThisVal = false,
                                              accessibility = None,
                                              range = Range.Zero
                                          ),
                                      returnInfo = None,
                                      expr = SynExpr.Const(constant = SynConst.Int32(12), range = Range.Zero),
                                      range = Range.Zero,
                                      debugPoint = DebugPointAtBinding.Yes(Range.Zero),
                                      trivia =
                                          { LeadingKeyword = SynLeadingKeyword.Let(Range.Zero)
                                            InlineKeyword = None
                                            EqualsRange = Some(Range.Zero) }
                                  ) ],
                            range = Range.Zero
                        ) ],
                  xmlDoc = PreXmlDoc.Empty,
                  attribs = [],
                  accessibility = None,
                  range = Range.Zero,
                  trivia = { LeadingKeyword = SynModuleOrNamespaceLeadingKeyword.None }
              ) ],
        flags = (false, false),
        trivia =
            { ConditionalDirectives = []
              CodeComments = [] },
        identifiers = set []
    )
)

// produces the following code:
let x = 12

(**
### Fantomas Oak AST

It is a simplified version of the official AST that is used by Fantomas to format F# code.

- It is more concise and easier to read.
- It is somewhat easier to manipulate or analyze programmatically.
- It is more human-readable, as it contains only the relevant information about the code itself.
- However, it still requires providing many optional values even for simple code.

You can see a live example using the [online tool](https://fsprojects.github.io/fantomas-tools/#/oak?data=N4KABGBEDGD2AmBTSAuKAbRAXMAPMAvGAIwBMkANOFEgGYCWAdogM6pSXWT0sBiL9drQCG6FoioRuLAOIAnYQAcAFgDV6iAO5DR4kAF8gA)
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"

open Fantomas.FCS.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak

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

// produces the following code:
(*** include-output ***)

(**
### Fabulous.AST DSL

Fabulous.AST provides a more user-friendly API to generate code using ASTs. It's built on top of Fantomas Oak AST and offers a more concise and easier-to-use API for code generation. It dramatically reduces the boilerplate code required to generate F# code.

You can configure code formatting using `FormatConfig`:

```fsharp
type FormatConfig =
    { IndentSize: Num
      MaxLineLength: Num
      EndOfLine: EndOfLineStyle
      InsertFinalNewline: bool
      SpaceBeforeParameter: bool
      SpaceBeforeLowercaseInvocation: bool
      SpaceBeforeUppercaseInvocation: bool
      SpaceBeforeClassConstructor: bool
      SpaceBeforeMember: bool
      SpaceBeforeColon: bool }
```

Here's the same example using Fabulous.AST:
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { AnonymousModule() { Value("x", "12") } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
## Getting Started

To start using Fabulous.AST, first install the NuGet package:

```
dotnet add package Fabulous.AST
```

Then, in your code:

```fsharp
open Fabulous.AST
open type Fabulous.AST.Ast

// Generate a simple module with a value
let code = 
    Oak() { 
        AnonymousModule() { 
            Value("x", Int(42)) 
        } 
    }
    |> Gen.mkOak
    |> Gen.run

printfn "%s" code
```

## Documentation Structure

Fabulous.AST documentation is organized by F# language features. Each page focuses on one specific aspect of F# code generation:

- **Modules and Namespaces**: How to create and organize code into modules and namespaces
- **Records**: Creating and working with record types
- **Discriminated Unions**: Generating sum types with different cases
- **Functions**: Defining and manipulating functions
- **Classes**: Creating classes with members and constructors
- **Expressions**: Building various expression types
- **Patterns**: Working with pattern matching
- **Type Definitions**: Creating different kinds of types
- **Units of Measure**: Defining and using units of measure

Each documentation page follows a consistent structure:

1. **Overview**: Brief explanation of the concept
2. **Basic Usage**: Simple examples of the most common usage
3. **API Reference**: Available widget constructors and modifiers
4. **Examples**: Practical examples showing different use cases
5. **Advanced Topics**: More complex scenarios and customizations

## What Widgets to Use to Generate Code?

Fabulous.AST maps to the Fantomas Oak AST nodes. You can use the [online tool](https://fsprojects.github.io/fantomas-tools/#/oak?data=N4KABGBEDGD2AmBTSAuKAbRAXMAPMAvGAIwBMAOgHaQA04USAZgJaWIDOqUt9kz7AMXbMujAIbp2iOhD7sA4gCcxABwAWANWaIA7qIlSQAXyA) to examine the AST nodes and then use the corresponding widgets to generate the code.

For example, the following Oak AST node:
```fsharp
Oak (1,0-1,10)
    ModuleOrNamespaceNode (1,0-1,10)
        BindingNode (1,0-1,10)
            MultipleTextsNode (1,0-1,3)
                let (1,0-1,3)
            IdentListNode (1,4-1,5)
                x (1,4-1,5)
            = (1,6-1,7)
            12 (1,8-1,10)
```

Translates to the following Fabulous.AST code:

```fsharp
Oak() {
    AnonymousModule() {
        Value("x", "12")
    }
}
```

We've reduced the boilerplate code from 70 lines to just 5 lines of code, making it much easier to read and understand.
*)
