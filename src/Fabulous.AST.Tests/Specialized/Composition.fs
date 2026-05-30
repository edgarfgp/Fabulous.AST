namespace Fabulous.AST.Tests.Specialized

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

/// Composition tests — exercise multiple widgets interacting in shapes that match
/// real F# code, beyond the per-widget happy-path tests in the rest of the suite.
module Composition =

    [<Fact>]
    let ``Interface with xmldocs, class with attributes, inheritance and interface impl``() =
        Oak() {
            AnonymousModule() {
                (TypeDefn("IShape") {
                    AbstractMember("Area", Float())
                    AbstractMember("Perimeter", Float())
                })
                    .xmlDocs([ "Shape with measurable area and perimeter." ])

                (TypeDefn("Shape", UnitPat()) {
                    InterfaceWith("IShape") {
                        Member("this.Area", ConstantExpr(Float 0.0))
                        Member("this.Perimeter", ConstantExpr(Float 0.0))
                    }
                })
                    .xmlDocs([ "Base shape with degenerate area / perimeter." ])
                    .attributes([ Attribute("Sealed") ])

                TypeDefn("Circle", ParenPat(ParameterPat("radius", Float()))) {
                    Inherit("Shape()")

                    InterfaceWith("IShape") {
                        Member(
                            "this.Area",
                            InfixAppExpr(
                                ConstantExpr(Constant "System.Math.PI"),
                                "*",
                                InfixAppExpr(ConstantExpr(Constant "radius"), "*", ConstantExpr(Constant "radius"))
                            )
                        )

                        Member(
                            "this.Perimeter",
                            InfixAppExpr(
                                InfixAppExpr(ConstantExpr(Float 2.0), "*", ConstantExpr(Constant "System.Math.PI")),
                                "*",
                                ConstantExpr(Constant "radius")
                            )
                        )
                    }
                }

            }
        }
        |> produces
            """
/// Shape with measurable area and perimeter.
type IShape =
    abstract Area: float
    abstract Perimeter: float

/// Base shape with degenerate area / perimeter.
[<Sealed>]
type Shape() =
    interface IShape with
        member this.Area = 0.0
        member this.Perimeter = 0.0

type Circle(radius: float) =
    inherit Shape()

    interface IShape with
        member this.Area = System.Math.PI * radius * radius
        member this.Perimeter = 2.0 * System.Math.PI * radius
"""

    [<Fact>]
    let ``Discriminated union with xmlDocs and a matching helper``() =
        Oak() {
            AnonymousModule() {
                (Union("Result") {
                    UnionCase("Ok", Field("value", "'T"))
                    UnionCase("Error", Field("error", "'E"))
                })
                    .typeParams(PostfixList([ "'T"; "'E" ]))
                    .xmlDocs([ "A computation that may fail with a typed error." ])

                Function(
                    "tryDivide",
                    [ ParameterPat("x"); ParameterPat("y") ],
                    IfThenElseExpr(
                        InfixAppExpr(ConstantExpr(Constant "y"), "=", ConstantExpr(Int 0)),
                        AppExpr(ConstantExpr(Constant "Error"), [ ConstantExpr(String "divide by zero") ]),
                        AppExpr(
                            ConstantExpr(Constant "Ok"),
                            [ ParenExpr(InfixAppExpr(ConstantExpr(Constant "x"), "/", ConstantExpr(Constant "y"))) ]
                        )
                    )
                )

            }
        }
        |> produces
            """
/// A computation that may fail with a typed error.
type Result<'T, 'E> =
    | Ok of value: 'T
    | Error of error: 'E

let tryDivide x y =
    if y = 0 then Error "divide by zero" else Ok (x / y)
"""

    [<Fact>]
    let ``Record with computed member and interface implementation``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IFormattable") { AbstractMember("Format", [ Unit() ], String()) }

                (Record("Person") {
                    Field("FirstName", String())
                    Field("LastName", String())
                    Field("Age", Int())
                })
                    .members() {
                    Member(
                        "this.FullName",
                        InfixAppExpr(
                            InfixAppExpr(ConstantExpr(Constant "this.FirstName"), "+", ConstantExpr(String " ")),
                            "+",
                            ConstantExpr(Constant "this.LastName")
                        )
                    )

                    InterfaceWith(LongIdent "IFormattable") {
                        Member(
                            "this.Format",
                            UnitPat(),
                            AppExpr(
                                ConstantExpr(Constant "sprintf"),
                                [ ConstantExpr(String "%s (%d)")
                                  ConstantExpr(Constant "this.FullName")
                                  ConstantExpr(Constant "this.Age") ]
                            )
                        )
                    }
                }
            }
        }
        |> produces
            """
type IFormattable =
    abstract Format: unit -> string

type Person =
    { FirstName: string
      LastName: string
      Age: int }

    member this.FullName = this.FirstName + " " + this.LastName

    interface IFormattable with
        member this.Format() =
            sprintf "%s (%d)" this.FullName this.Age
"""

    [<Fact>]
    let ``Namespace with nested module containing abbrevs and functions``() =
        Oak() {
            Namespace("MyApp.Domain") {
                Module("Money") {
                    Abbrev("Cents", Int())
                    Abbrev("Dollars", Float())

                    Function(
                        "toCents",
                        [ ParameterPat("d") ],
                        AppExpr(
                            ConstantExpr(Constant "int"),
                            [ ParenExpr(InfixAppExpr(ConstantExpr(Constant "d"), "*", ConstantExpr(Float 100.0))) ]
                        )
                    )

                    Function(
                        "toDollars",
                        [ ParameterPat("c") ],
                        InfixAppExpr(
                            ParenExpr(AppExpr(ConstantExpr(Constant "float"), [ ConstantExpr(Constant "c") ])),
                            "/",
                            ConstantExpr(Float 100.0)
                        )
                    )
                }
            }
        }
        |> produces
            """
namespace MyApp.Domain

module Money =
    type Cents = int
    type Dollars = float
    let toCents d = int (d * 100.0)
    let toDollars c = (float c) / 100.0
"""

    [<Fact>]
    let ``Mutually recursive union types with `and` keyword``() =
        Oak() {
            AnonymousModule() {
                (Union("Tree") {
                    UnionCase("Leaf")
                    UnionCase("Branch", Field("forest", LongIdent "Forest"))
                })
                    .typeParams(PostfixList([ "'a" ]))

                (Union("Forest") {
                    UnionCase("Empty")

                    UnionCase(
                        "NonEmpty",
                        Field("trees", AppPrefix(LongIdent "list", [ AppPrefix(LongIdent "Tree", [ LongIdent "'a" ]) ]))
                    )
                })
                    .typeParams(PostfixList([ "'a" ]))
                |> _.toRecursive()
            }
        }
        |> produces
            """
type Tree<'a> =
    | Leaf
    | Branch of forest: Forest

and Forest<'a> =
    | Empty
    | NonEmpty of trees: list<Tree<'a>>
"""
