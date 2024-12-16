(**
---
title: Homepage
category: docs
index: 0
---

# Fabulous.AST

#### What is AST?

`AST` stands for Abstract Syntax Tree. It is a tree representation of the abstract syntactic structure of source code written in a programming language.
It is used by compilers to analyze, transform, and generate code.

#### Can I generate code without using AST?
You can generate code by just using strings and string interpolation. But there are several reasons why you should not do that.

Here are some of the reasons:

- It's error-prone and hard to maintain.
- If the code you are generating is complex, then it's even harder to generate it using string interpolation.
- You will have to write some extra code to handle edge cases and make sure that the generated code is valid, handle formatting and indentation.

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

Quote from fantomas:

> For mercy's sake don't use string concatenation when generating F# code, use Fantomas instead. It is battle tested and proven technology!

#### Why use AST?

ASTs are more verbose than string interpolation and requires you to think in terms of nodes and trees, which can be a bit hard to grasp at first, but it becomes very powerful when you use it to generate code.

Some of the benefits of using AST to generate code are:

- The code generated is compliant with the F# syntax and is will most likely be valid and compilable.
- It provides a more structured way to generate code.
- It allows you to manipulate the code programmatically.
- It allows you to generate more complex code in a more maintainable way.

#### How to use AST to generate code?

Let's explore three different ways to generate code using AST:

##### 1. Compiler AST

- It is a very verbose and hard to read.
- It is hard manipulate or analyze programmatically.
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
##### 2. Fantomas Oak AST

It is a simplified version of the official AST that is used by Fantomas to format F# code.

- It is more concise and easier to read.
- It is a bit easier to manipulate or analyze programmatically.
- It is a bit more human-readable. as it contains only the relevant information about the code itself.
- But it's still not very easy to work with, as we will need to provide a lot of optional values even for simple code.

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

##### 3. Fabulous.AST DSL

So far we have seen how to generate code using the official F# compiler AST and Fantomas Oak AST.
You might be thinking that it's not worth the effort to use ASTs to generate code and still prefer string interpolation. But that's where `Fabulous.AST` comes in.

`Fabulous.AST` library provides a more user-friendly API to generate code using ASTs.
It is built on top of Fantomas Oak AST and provides a more concise and easier to use API to generate code.
It aims to really cut down on the boilerplate code required to generate code.

You can provide your own configuration to format the code as you like by using `FormatConfig`.

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

Now let's take a look at same example using Fabulous.AST:
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
#### What widgets to use to generate the code?

Fabulous.AST maps the Fantomas Oak AST nodes. So you can use the [online tool](https://fsprojects.github.io/fantomas-tools/#/oak?data=N4KABGBEDGD2AmBTSAuKAbRAXMAPMAvGAIwBMAOgHaQA04USAZgJaWIDOqUt9kz7AMXbMujAIbp2iOhD7sA4gCcxABwAWANWaIA7qIlSQAXyA) to take a peak at the AST nodes and then use the corresponding widgets to generate the code.

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

We have cut down the boilerplate code from 70 lines to just 5 lines of code. And it's much easier to read and understand.
But we can generate much more complex code using Fabulous.AST. Let's take a look at some more examples.

*)

(**
#### Example 1: Modules and Namespaces

- We will generate a namespace `Widgets` with a module `WidgetsModule` and a value `x` with the value `12`.
- We will generate an implicit namespace `Widgets.WidgetModule` with a function `widgetFunction` with two parameters `x` and `y` and the body `12`.

To learn more about modules and namespaces in F#, you can read the [official documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/namespaces).
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

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

(**
#### Example 2: Records
- We will generate a record type with two fields `Name` and `Age`.
- We will use `Namespace`, `Module` and `Record` widgets to generate the code.
- We want the record to be formatted using `Stroustrup` style.
- Create a person record instance with name `Jose` and age `30`.

To learn more about records in F#, you can read the [official documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/records).
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

let formatConfig =
    { FormatConfig.Default with
        MultilineBracketStyle = Stroustrup
        RecordMultilineFormatter = NumberOfItems }

Oak() {
    Namespace("MyNamespace") {
        Module("Records") {
            (Record("Person") {
                Field("Name", "string")
                Field("Age", "int")
            })
                .members() {
                Member("this.Name", InterpolatedStringExpr([ Constant("this.Name"); Constant("this.Age") ]))
            }

            Value("person", RecordExpr([ RecordFieldExpr("Name", "Jose"); RecordFieldExpr("Age", "30") ]))

            Record("Person") {
                Field("Name", "string")
                Field("Age", "int")
            }
            |> _.attribute(Attribute("Struct"))
        }
    }
}
|> Gen.mkOak
|> Gen.runWith formatConfig
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
#### Example 3: Discriminated unions
- We will generate a discriminated union type with two cases `Some 'a` and `None`.
- We will use AnonymousModule, Union and UnionCase widgets to generate the code.
- We will provide xmlDocs for the union type and the union cases.

To learn more about discriminated unions in F#, you can read the [official documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Option") {
            UnionCase("Some", "'a")
                .triviaBefore(SingleLine("Represents the Some case with a value."))

            UnionCase("None").triviaBefore(SingleLine("Represents the None case."))
        }
        |> _.typeParams(PostfixList("'a"))
        |> _.xmlDocs([ "Represents the option type." ])

        Union("Option") {
            UnionCase("Some", "'a")
                .triviaBefore(SingleLine("Represents the Some case with a value."))

            UnionCase("None").triviaBefore(SingleLine("Represents the None case."))
        }
        |> _.attribute(Attribute("Struct"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
#### Example 4: Classes
- We will generate a class type with explicit constructor with two fields `Name`, LastName` and `Age`.
- We will use AnonymousModule, TypeDefn and Member widgets to generate the code.

To learn more about classes in F#, you can read the [official documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/classes).
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        TypeDefn(
            "Person",
            ParenPat(
                TuplePat(
                    [ ParameterPat(ConstantPat(Constant("name")))
                      ParameterPat(ConstantPat(Constant("lastName")))
                      ParameterPat(ConstantPat(Constant("age"))) ]
                )
            )
        ) {
            Member("this.Name", ConstantExpr("name"))
            Member("this.LastName", ConstantExpr("lastName"))
            Member("this.Age", ConstantExpr("age"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
#### Example 4: Units of Measure
- We will generate a unit of measure type with 6 cases `cm`, `ml`, `m`, `s`, `kg` and `N`.
- We will use AnonymousModule, Measure, MeasurePower, Tuple, AppPostfix and LongIdent widgets to generate the code.
- We will provide xmlDocs for each unit of measure.

To learn more about units of measure in F#, you can read the [official documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/units-of-measure).
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Measure("cm").xmlDocs([ "Cm, centimeters." ])

        Measure("ml", MeasurePower(LongIdent "cm", Integer "3"))
            .xmlDocs([ "Ml, milliliters." ])

        Measure("m").xmlDocs([ "Distance, meters." ])

        Measure("s").xmlDocs([ "Time, seconds." ])

        Measure("kg").xmlDocs([ "Mass, kilograms." ])

        Measure(
            "N",
            Tuple(
                [ AppPostfix(LongIdent "kg", LongIdent "m")
                  MeasurePower(LongIdent "s", Integer "2") ],
                "/"
            )
        )
            .xmlDocs([ "Force, Newtons." ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**

#### Conclusion
Hope by now you have a good understanding of what is an AST and how you can use it to generate code using `Fabulous.AST`.

You can generate pretty much any F# code you want as long as you provide the correct widgets.
Use the [online tool](https://fsprojects.github.io/fantomas-tools/#/oak?data=N4KABGBEDGD2AmBTSAuKAbRAXMAPMAvGAIwBMAOgHaQA04USAZgJaWIDOqUt9kz7AMXbMujAIbp2iOhD7sA4gCcxABwAWANWaIA7qIlSQAXyA) to find the correct widgets for the code you want to generate.

#### References
- [FSharp.Compiler.Service](https://fsharp.github.io/fsharp-compiler-docs/fcs/untypedtree-apis.html)
- [Generating source code](https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html)
- [Online tool](https://fsprojects.github.io/fantomas-tools/)
*)
