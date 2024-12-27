(**
---
title: Modules
category: widgets
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
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

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
        |> _.triviaBefore(Newline())

        Module("MyModule2") {
            Value("module2Value", Int(121))
                .triviaBefore(
                    SingleLine("Indent all program elements within modules that are declared with an equal sign.")
                )

            Function(
                "module2Function",
                ParameterPat("x"),
                InfixAppExpr("x", "*", ParenExpr(AppExpr("MyModule1.module1Function", "module2Value")))
            )
                .triviaBefore(
                    SingleLine("Indent all program elements within modules that are declared with an equal sign.")
                )
        }
    }

    Namespace("Arithmetic") {
        Function("add", [ ParameterPat("x"); ParameterPat("y") ], InfixAppExpr("x", "+", "y"))
        Function("sub", [ ParameterPat("x"); ParameterPat("y") ], InfixAppExpr("x", "-", "y"))

    }
    |> _.toImplicit()
    |> _.triviaBefore(Newline())
    |> _.triviaBefore(
        BlockComment(
            """

Referencing Code in Modules
When you reference functions, types, and values from another module, you must either use a qualified name or open the module.
If you use a qualified name, you must specify the namespaces, the module, and the identifier for the program element you want.
You separate each part of the qualified path with a dot (.), as follows.

Namespace1.Namespace2.ModuleName.Identifier
"""
        )
    )

    AnonymousModule() {
        Value("result1", AppExpr("Arithmetic.add", [ Int(5); Int(9) ]))
            .triviaBefore(SingleLine("Fully qualify the function name."))

        Open("Arithmetic").triviaBefore(SingleLine("Open the module."))

        Value("result2", AppExpr("add", [ Int(5); Int(9) ]))
    }
    |> _.triviaBefore(Newline())

    AnonymousModule() {
        Module("Y") {
            Value("x", Int(1))

            Module("Z") { Value("z", Int(5)).triviaAfter(Newline()) }
        }
        |> _.triviaBefore(
            BlockComment(
                """
Nested Modules
Modules can be nested. Inner modules must be indented as far as outer module declarations to indicate that they are inner modules, not new modules.
For example, compare the following two examples. Module Z is an inner module in the following code.
"""
            )
        )
    }
    |> _.triviaBefore(Newline())

    AnonymousModule() {
        Module("Y") { Value("x", Int(1)) }

        Module("Z") { Value("z", Int(5)) }
    }
    |> _.triviaBefore(SingleLine("Nested Modules"))
    |> _.triviaBefore(Newline())

    AnonymousModule() {
        Module("Y") {
            Value("x", Int(1))

            Module("Z") { Value("z", Int(5)) }
        }

    }
    |> _.triviaBefore(SingleLine("Nested Modules"))
    |> _.triviaBefore(Newline())

    AnonymousModule() { Module("Y") { Module("Z") { Value("z", Int(5)) } } }
    |> _.triviaBefore(SingleLine("Nested Modules"))
    |> _.triviaBefore(Newline())

    Namespace("TopLevel") {
        Value("topLevelX", Int(5))

        Module("Inner1") { Value("inner1X", Int(1)) }

        Module("Inner2") { Value("inner2X", Int(5)) }
    }
    |> _.toImplicit()
    |> _.triviaBefore(
        BlockComment(
            """
The top-level module declaration can be omitted if the file is named
TopLevel.fs or topLevel.fs, and the file is the only file in an
application.
"""
        )
    )
    |> _.triviaBefore(Newline())

    AnonymousModule() {
        Module("RecursiveModule") {
            Union("Orientation") {
                UnionCase("Up")
                UnionCase("Down")
            }

            Union("PeelState") {
                UnionCase("Peeled")
                UnionCase("Unpeeled")
            }

            ExceptionDefn("DontSqueezeTheBananaException", Field("Banana"))

            TypeDefn("Banana", Constructor(ParenPat(ParameterPat("orientation", LongIdent("Orientation"))))) {
                MemberVal("IsPeeled", Bool(false), true, true)
                MemberVal("Orientation", "orientation", true, true)

                MemberVal("Sides", ListExpr([ "PeelState"; "Unpeeled" ]), true, true)
                    .returnType(LongIdent "PeelState list")

                Member("self.Peel", UnitPat(), "BananaHelpers.peel")
                    .triviaAfter(LineCommentAfterSourceCode("Note the dependency on the BananaHelpers module."))

                Member("self.SqueezeJuiceOut", UnitPat(), "raise (DontSqueezeTheBananaException self)")
                    .triviaAfter(LineCommentAfterSourceCode("This member depends on the exception above."))
            }

            Module("BananaHelpers") {
                Function(
                    "peel",
                    [ ParenPat(ParameterPat("b", "Banana")) ],
                    CompExprBodyExpr(
                        [ Function(
                              "flip",
                              [ ParenPat(ParameterPat("banana", "Banana")) ],
                              MatchExpr(
                                  "banana.Orientation",
                                  [ MatchClauseExpr(
                                        "Up",
                                        CompExprBodyExpr(
                                            [ LongIdentSetExpr("banana.Orientation", "Down"); ConstantExpr("banana") ]
                                        )
                                    )

                                    MatchClauseExpr("Down", "banana") ]
                              )
                          )
                          |> LetOrUseExpr

                          Function(
                              "peelSides",
                              [ ParenPat(ParameterPat("banana", "Banana")) ],
                              InfixAppExpr(
                                  "banana.Sides",
                                  "|>",
                                  MatchLambdaExpr(
                                      [ MatchClauseExpr("Unpeeled", "Peeled"); MatchClauseExpr("Peeled", "Peeled") ]
                                  )
                              )
                          )
                          |> LetOrUseExpr

                          MatchExpr(
                              "b.Orientation",
                              [ MatchClauseExpr("Up", SameInfixAppsExpr("b", [ ("|>", "flip"); ("|>", "peelSides") ]))

                                MatchClauseExpr("Down", InfixAppExpr("b", "|>", "peelSides")) ]
                          )
                          |> OtherExpr ]
                    )
                )
            }
            |> _.triviaBefore(Newline())
            |> _.triviaBefore(SingleLine("Mutual References"))
            |> _.triviaAfter(Newline())
            |> _.triviaAfter(
                BlockComment(
                    """
Note that the exception DontSqueezeTheBananaException and the class Banana both refer to each other.
Additionally, the module BananaHelpers and the class Banana also refer to each other.
This would not be possible to express in F# if you removed the rec keyword from the RecursiveModule module.
"""
                )
            )

        }
        |> _.toRecursive()
    }
    |> _.triviaBefore(
        BlockComment(
            """
Recursive modules
F# 4.1 introduces the notion of modules which allow for all contained code to be mutually recursive.
This is done via module rec.
Use of module rec can alleviate some pains in not being able to write mutually referential code between types and modules.
The following is an example of this:
"""
        )
    )
    |> _.triviaBefore(Newline())

}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
