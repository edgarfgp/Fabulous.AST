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

(**
# Generated code:
*)

type Option<'a> =
    | Some of 'a
    | None

type Shape =
    | Line
    | Rectangle of width: float * length: float
    | Circle of radius: float
    | Prism of width: float * float * height: float

    member this.Area() =
        match this with
        | Line -> 0.0
        | Rectangle(width, length) -> width * length
        | Circle(radius) -> System.Math.PI * radius * radius
        | Prism(width, length, height) -> width * length * height

type Color =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Yellow = 3

type SizeType = uint32
type Transform<'a> = 'a -> 'a

[<Measure>]
type cm

[<Measure>]
type ml = cm^3

[<Measure>]
type m

[<Measure>]
type s

[<Measure>]
type kg

[<Measure>]
type N = kg m / s^2

type Point =
    { X: float
      Y: float
      Z: float }

[<Struct>]
type StructPoint =
    { X: float
      Y: float
      Z: float }

type Person<'other> =
    { Name: 'other }

    member this.GetName = "name"

type Person2(name: string, lastName: string, ?age: int) =
    member this.Name = name

type Person3<'a, 'b>() =
    member this.Name = ""

type Variant =
    | Num of int
    | Str of string

module Variant =
    let print v =
        match v with
        | Num n -> printf "Num %d" n
        | Str s -> printf "Num %s" s

type Variant with
    member x.Print() = Variant.print x
