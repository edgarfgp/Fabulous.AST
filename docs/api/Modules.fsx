(**
---
title: Modules
category: api
index: 3
---
*)

(**
# Modules
For details on how the AST node works, please refer to the [Fantomas documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-moduleornamespacenode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/modules) for a comparison between the two.

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Module(name: string)                             | Creates a Module AST node with the specified name. |
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
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.Builders.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {

        Value("x", Int(1))
            .triviaBefore(
                BlockComment(
                    """
If a code file does not begin with a top-level module declaration or a namespace declaration,
the whole contents of the file, including any local modules,
becomes part of an implicitly created top-level module that has the same name as the file,
without the extension, with the first letter converted to uppercase. For example, consider the following file.
"""
                )
            )
            .triviaBefore(Newline())
            .triviaAfter(Newline())
    }

    Namespace("Program") { Value("y", Int(2)) }
    |> _.triviaBefore(SingleLine("Top-level module declaration."))
    |> _.toImplicit()

    AnonymousModule() {
        Module("MyModule1") {
            Value("module1Value", Int(100))
                .triviaBefore(
                    SingleLine("Indent all program elements within modules that are declared with an equal sign.")
                )

            Function("module1Function", ParameterPat("x"), InfixAppExpr("x", "+", Int(1)))
                .triviaBefore(
                    SingleLine("Indent all program elements within modules that are declared with an equal sign.")
                )
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
