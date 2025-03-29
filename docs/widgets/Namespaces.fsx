(**
---
title: Namespaces
category: widgets
index: 2
---
*)

(**
# Namespaces

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Implicit Namespaces](#implicit-namespaces)
- [Global Namespace](#global-namespace)
- [Multiple Modules in a Namespace](#multiple-modules-in-a-namespace)
- [Multiple Namespaces](#nested-namespaces)
- [Recursive Namespaces](#recursive-namespaces)

## Overview
Namespaces in F# are a way to organize code by grouping related types and modules. They help prevent naming conflicts
and provide a hierarchical structure for organizing code. Unlike modules, namespaces can be split across multiple files and assemblies.

## Basic Usage
Create a namespace with the `Namespace` widget:
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
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Implicit Namespaces
Implicit namespaces are created using the `toImplicit` modifier:
*)

Oak() {
    Namespace("Widgets.WidgetModule") {
        Function(
            "widgetFunction",
            [ ParameterPat("x"); ParameterPat("y") ],
            AppExpr("printfn", [ String("%A %A"); String("x"); String("y") ])
        )
    }
    |> _.toImplicit()
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Global Namespace
The global namespace can be accessed using the `GlobalNamespace` widget:
*)

Oak() { GlobalNamespace() { TypeDefn("MyClass", UnitPat()) { Member("this.Prop1", String("X")) } } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Multiple Modules in a Namespace
A namespace can contain multiple modules:
*)

Oak() {
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
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Multiple Namespaces
Namespaces can be nested to create a hierarchical structure:
*)

Oak() {
    Namespace("Outer") {
        TypeDefn("MyClass", UnitPat()) {
            Member("this.X", ParenPat(ParameterPat(ConstantPat(Constant "x"))), InfixAppExpr("p", "+", Int(1)))
        }
    }

    Namespace("Outer.Inner") { TypeDefn("MyClass", UnitPat()) { Member("this.Prop1", String("X")) } }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Recursive Namespaces
Create recursive namespaces with the `toRecursive` method. This is useful when types and modules within need to reference each other:
*)

Oak() {
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

            MemberVal(
                "Sides",
                ListExpr([ "PeelState"; "Unpeeled" ]),
                returnType = LongIdent("PeelState list"),
                hasGetter = true,
                hasSetter = true
            )

            Member("self.Peel", UnitPat(), "BananaHelpers.peel")

            Member("self.SqueezeJuiceOut", UnitPat(), "raise (DontSqueezeTheBananaException self)")
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
    }
    |> _.toRecursive()
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
