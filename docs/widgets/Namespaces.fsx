(**
---
title: Namespaces
category: widgets
index: 2
---
*)

(**
# Namespaces
For details on how the AST node works, please refer to the [Fantomas documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-moduleornamespacenode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/namespaces) for a comparison between the two.

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Namespace(name: string)                  | Creates a NamespaceNode AST node with the specified name. |
| Namespace(name: string)          | Creates an implicit NamespaceNode AST node with the specified name
| AnonymousModule()                        | Creates an NamespaceNode AST node. |
*)

(**
### Modifier Functions

| Modifier                                   | Description                                                                                     |
|---------------------------------------------|-------------------------------------------------------------------------------------------------|
| toRecursive(this: WidgetBuilder<ModuleOrNamespaceNode>) | Adds a scalar indicating that the namespace is recursive.                                         |
| toPrivate(this: WidgetBuilder<ModuleOrNamespaceNode>)   | Sets the accessibility of the namespace to private.                                               |
| toPublic(this: WidgetBuilder<ModuleOrNamespaceNode>)    | Sets the accessibility of the namespace to public.                                                |
| toInternal(this: WidgetBuilder<ModuleOrNamespaceNode>)  | Sets the accessibility of the namespace to internal.
| toImplicit(this: WidgetBuilder<ModuleOrNamespaceNode>)  | Sets the namespace to be implicit.
| xmlDocs(this: WidgetBuilder<ModuleOrNamespaceNode>, xmlDocs: string list) | Adds XML documentation comments to the module.                                                |
| attributes(this: WidgetBuilder<ModuleOrNamespaceNode>, attributes: WidgetBuilder<AttributeNode> list) | Adds multiple attributes to the module.                                                        |
| attribute(this: WidgetBuilder<ModuleOrNamespaceNode>, attribute: WidgetBuilder<AttributeNode>) | Adds a single attribute to the module.                                                         |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    Namespace("Widgets") {
        TypeDefn("MyWidget1") { Member("this.WidgetName", String("Widget1")) }

        Module("WidgetsModule") { Value("widgetName", String("Widget2")) }
    }
    |> _.triviaBefore(SingleLine("Namespace"))
    |> _.triviaAfter(Newline())

    Namespace("Widgets.WidgetModule") {
        Function(
            "widgetFunction",
            [ ParameterPat("x"); ParameterPat("y") ],
            AppExpr("printfn", [ String("%A %A"); String("x"); String("y") ])
        )
    }
    |> _.toImplicit()
    |> _.triviaBefore(SingleLine("Implicit Namespace"))
    |> _.triviaAfter(Newline())

    Namespace("Widgets") {
        Module("WidgetModule1") {
            Function(
                "widgetFunction",
                [ ParameterPat("x"); ParameterPat("y") ],
                AppExpr("printfn", [ String("Module1 %A %A"); String("x"); String("y") ])
            )
        }

        Module("WidgetModule2") {
            Function(
                "widgetFunction",
                [ ParameterPat("x"); ParameterPat("y") ],
                AppExpr("printfn", [ String("Module2 %A %A"); String("x"); String("y") ])
            )
        }

        Module("useWidgets") {
            SingleExpr(
                "do",
                CompExprBodyExpr(
                    [ AppExpr("WidgetModule1.widgetFunction", [ Int(10); Int(20) ])
                      AppExpr("WidgetModule2.widgetFunction", [ Int(5); Int(6) ]) ]
                )
            )
        }
    }
    |> _.triviaBefore(SingleLine("Multiple Modules"))
    |> _.triviaAfter(Newline())

    Namespace("Widgets") {
        TypeDefn("MyWidget1") { Member("this.WidgetName", String("Widget1")) }

        Module("WidgetsModule") { Value("widgetName", String("Widget2")) }
    }
    |> _.triviaBefore(SingleLine("Namespace"))
    |> _.triviaAfter(Newline())

    Namespace("Outer") {
        TypeDefn("MyClass", ParenPat()) {
            Member("this.X", ParenPat(ParameterPat(ConstantPat(Constant "x"))), InfixAppExpr("p", "+", Int(1)))
        }
        |> _.triviaBefore(SingleLine("Full name: Outer.MyClass"))
    }
    |> _.triviaBefore(SingleLine("Nested Namespaces"))
    |> _.triviaAfter(Newline())

    Namespace("Outer.Inner") { TypeDefn("MyClass", ParenPat()) { Member("this.Prop1", String("X")) } }
    |> _.triviaBefore(SingleLine("Full name: Outer.Inner.MyClass"))
    |> _.triviaAfter(Newline())

    GlobalNamespace() { TypeDefn("MyClass", ParenPat()) { Member("this.Prop1", String("X")) } }
    |> _.triviaBefore(SingleLine("Global Namespace"))
    |> _.triviaAfter(Newline())

    AnonymousModule() { Value("x", ConstantExpr(Int(3))) }
    |> _.triviaBefore(SingleLine("Anonymous Module"))
    |> _.triviaAfter(Newline())

    Namespace("MutualReferences") {
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

            Member("self.Peel", ParenPat(), "BananaHelpers.peel")
                .triviaAfter(LineCommentAfterSourceCode("Note the dependency on the BananaHelpers module."))

            Member("self.SqueezeJuiceOut", ParenPat(), "raise (DontSqueezeTheBananaException self)")
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
This wouldn't be possible to express in F# if you removed the rec keyword from the MutualReferences namespace.
"""
            )
        )

    }
    |> _.toRecursive()
    |> _.triviaBefore(SingleLine("Recursive Namespace"))
    |> _.triviaAfter(Newline())

}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
