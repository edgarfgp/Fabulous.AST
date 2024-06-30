(**
---
title: TypeDefinitions
category: widgets
index: 5
---
*)

(**
# Type Definitions
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Option") {
            UnionCase("Some", "'a")
            UnionCase("None")
        }
        |> _.typeParams(PostfixList("'a"))

        (Union("Shape") {
            UnionCase("Line")
            UnionCase("Rectangle", [ Field("width", Float()); Field("length", Float()) ])
            UnionCase("Circle", Field("radius", Float()))
            UnionCase("Prism", [ Field("width", Float()); Field("float"); Field("height", Float()) ])
        })
            .members() {
            Method(
                "this.Area",
                UnitPat(),
                MatchExpr(
                    "this",
                    [ MatchClauseExpr("Line", Float(0.0))
                      MatchClauseExpr("Rectangle(width, length)", InfixAppExpr("width", "*", "length"))

                      MatchClauseExpr(
                          LongIdentPat("Circle", ParenPat("radius")),
                          SameInfixAppsExpr("System.Math.PI", [ ("*", "radius"); ("*", "radius") ])
                      )

                      MatchClauseExpr(
                          LongIdentPat("Prism", ParenPat(TuplePat([ "width"; "length"; "height" ]))),
                          SameInfixAppsExpr("width", [ ("*", "length"); ("*", "height") ])
                      ) ]
                )
            )
        }

        Enum("Color") {
            EnumCase("Red", Int(0))
            EnumCase("Green", Int(1))
            EnumCase("Blue", Int(2))
            EnumCase("Yellow", Int(3))
        }

        Abbrev("SizeType", UInt32())

        Abbrev("Transform", Funs("'a", "'a")).typeParams(PostfixList("'a"))

        Measure("cm")

        Measure("ml", MeasurePower(LongIdent "cm", Integer "3"))

        Measure("m")

        Measure("s")

        Measure("kg")

        Measure(
            "N",
            Tuple(
                [ AppPostfix(LongIdent "kg", LongIdent "m")
                  MeasurePower(LongIdent "s", Integer "2") ],
                "/"
            )
        )

        Record("Point") {
            Field("X", Float())
            Field("Y", Float())
            Field("Z", Float())
        }

        Record("StructPoint") {
            Field("X", Float())
            Field("Y", Float())
            Field("Z", Float())
        }
        |> _.attribute(Attribute "Struct")

        (Record("Person") { Field("Name", LongIdent("'other")) })
            .typeParams(PostfixList([ "'other" ]))
            .members() {
            Property(ConstantPat(Constant("this.GetName")), ConstantExpr(String "name"))
        }

        Class(
            "Person2",
            ImplicitConstructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat(ConstantPat(Constant("name")), String())
                          ParameterPat(ConstantPat(Constant("lastName")), String())
                          ParameterPat(ConstantPat(Constant("?age")), Int()) ]
                    )
                )
            )
        ) {
            Property(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
        }

        Class("Person3") { Property(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
        |> _.typeParams(PostfixList([ "'a"; "'b" ]))

        Union("Variant") {
            UnionCase("Num", Int())
            UnionCase("Str", String())
        }

        NestedModule("Variant") {
            Function(
                "print",
                "v",
                MatchExpr(
                    "v",
                    [ MatchClauseExpr("Num n", "printf \"Num %d\" n")
                      MatchClauseExpr("Str s", "printf \"Num %s\" s") ]
                )
            )
        }

        Augmentation("Variant") { Method("x.Print", UnitPat(), AppExpr("Variant.print", "x")) }
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

// produces the following code:
(*** include-output ***)
