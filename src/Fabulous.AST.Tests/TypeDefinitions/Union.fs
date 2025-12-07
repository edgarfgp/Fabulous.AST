namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit

open type Fabulous.AST.Ast

module Union =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "`` Red Blue ``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "`` net6.0 ``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces an union with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { Union("Colors") { UnionCase(value) } } }
        |> produces
            $$"""

type Colors = | {{expected}}

"""

    [<Fact>]
    let ``Produces an union``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red")
                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                }
            }
        }

        |> produces
            """

type Colors =
    | Red
    | Green
    | Blue
    | Yellow

"""

    [<Fact>]
    let ``Produces a generic union``() =
        Oak() {
            AnonymousModule() {
                Union("Option") {
                    UnionCase("Some", "'a")
                    UnionCase("None")
                }
                |> _.typeParams(PostfixList("'a"))
            }
        }

        |> produces
            """
type Option<'a> =
    | Some of 'a
    | None

"""

    [<Fact>]
    let ``Produces an union with interface member``() =
        Oak() {
            AnonymousModule() {

                TypeDefn("IMyInterface") {
                    let parameters = [ Unit() ]
                    AbstractMember("GetValue", parameters, String())
                }

                (Union("Colors") {
                    UnionCase("Red")
                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .members() {
                    InterfaceWith("IMyInterface") {
                        Member(ConstantPat(Constant("x.GetValue")), ConstantExpr(String ""))
                    }
                }
            }
        }
        |> produces
            """
type IMyInterface =
    abstract GetValue: unit -> string

type Colors =
    | Red
    | Green
    | Blue
    | Yellow

    interface IMyInterface with
        member x.GetValue = ""

"""

    [<Fact>]
    let ``Produces an union with multiple fields``() =
        Oak() {
            AnonymousModule() {
                Union("Shape") {
                    UnionCase("Rectangle", [ Field(Float()); Field(Float()) ])
                    UnionCase("Rectangle", Float())
                    UnionCase("Rectangle", [ "float"; "float" ])
                    UnionCase("Rectangle", Field(Float()))
                    UnionCase("Rectangle", Field("width", Float()))
                    UnionCase("Rectangle", [ Field("width", Float()); Field("height", Float()) ])
                    UnionCase("Rectangle", [ ("width", "float"); ("height", "float") ])
                    UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
                }
            }
        }
        |> produces
            """
type Shape =
    | Rectangle of float * float
    | Rectangle of float
    | Rectangle of float * float
    | Rectangle of float
    | Rectangle of width: float
    | Rectangle of width: float * height: float
    | Rectangle of width: float * height: float
    | Rectangle of width: float * height: float

"""

    [<Fact>]
    let ``Produces an union with fields``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field("b", LongIdent "int") ])

                    UnionCase("Green", [ Field(String()); Field(Int()) ])
                    UnionCase("Blue")
                    UnionCase("Yellow")
                }
            }
        }
        |> produces
            """

type Colors =
    | Red of a: string * b: int
    | Green of string * int
    | Blue
    | Yellow

"""

    [<Fact>]
    let ``Produces recursive unions``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field("b", LongIdent "int") ])

                    UnionCase("Green", [ Field(String()); Field(Int()) ])
                    UnionCase("Blue")
                    UnionCase("Yellow")
                }

                Union("Shapes") {
                    UnionCase("Circle", [ Field("radius", Float()) ])
                    UnionCase("Rectangle", [ Field("width", LongIdent "float"); Field("height", LongIdent "float") ])

                    UnionCase(
                        "Triangle",
                        [ Field("a", LongIdent "float")
                          Field("b", LongIdent "float")
                          Field("c", LongIdent "float") ]
                    )
                }
                |> _.toRecursive()
            }
        }
        |> produces
            """

type Colors =
    | Red of a: string * b: int
    | Green of string * int
    | Blue
    | Yellow

and Shapes =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle of a: float * b: float * c: float

"""

    [<Fact>]
    let ``Produces an union with SingleTextNode``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red")
                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                }
            }
        }
        |> produces
            """

type Colors =
    | Red
    | Green
    | Blue
    | Yellow

"""

    [<Fact>]
    let ``Produces an union using Widget and escape hatch``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red")
                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                    EscapeHatch(UnionCaseNode(None, None, None, SingleTextNode("Black", Range.Zero), [], Range.Zero))
                }
            }
        }
        |> produces
            """

type Colors =
    | Red
    | Green
    | Blue
    | Yellow
    | Black

"""

    [<Fact>]
    let ``Produces an union with attribute``() =
        Oak() { AnonymousModule() { (Union("Colors") { UnionCase("Red") }).attribute(Attribute "Test") } }
        |> produces
            """

[<Test>]
type Colors = | Red
"""

    [<Fact>]
    let ``Produces an union case with attributes``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") { UnionCase("Red").attributes([ Attribute("Obsolete"); Attribute("Test") ]) })
                    .attribute(Attribute "Test")
            }
        }
        |> produces
            """

[<Test>]
type Colors = | [<Obsolete; Test>] Red
"""

    [<Fact>]
    let ``yield! multiple unions``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ Union("Colors") {
                          UnionCase("Red")
                          UnionCase("Green")
                      }
                      Union("Shapes") {
                          UnionCase("Circle")
                          UnionCase("Square")
                      } ]
            }
        }
        |> produces
            """
type Colors =
    | Red
    | Green

type Shapes =
    | Circle
    | Square
"""

    [<Fact>]
    let ``yield! multiple union cases``() =
        Oak() {
            AnonymousModule() { Union("Colors") { yield! [ UnionCase("Red"); UnionCase("Green"); UnionCase("Blue") ] } }
        }
        |> produces
            """
type Colors =
    | Red
    | Green
    | Blue
"""

module GenericUnion =

    [<Fact>]
    let ``Produces an union with TypeParams``() =
        Oak() {
            AnonymousModule() {
                Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field("b", LongIdent("'other")) ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                }
                |> _.typeParams(PostfixList([ "'other" ]))
            }
        }
        |> produces
            """

type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

"""

    [<Fact>]
    let ``Produces an union with TypeParams and interface member``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMyInterface") { AbstractMember("GetValue", [ Unit() ], String()) }

                (Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field(LongIdent("'other")) ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    InterfaceWith(LongIdent "IMyInterface") {
                        Member(ConstantPat(Constant("x.GetValue")), ConstantExpr(String ""))
                    }
                }
            }
        }

        |> produces
            """
type IMyInterface =
    abstract GetValue: unit -> string

type Colors<'other> =
    | Red of a: string * 'other
    | Green
    | Blue
    | Yellow

    interface IMyInterface with
        member x.GetValue = ""

"""

    [<Fact>]
    let ``Produces an struct union with TypeParams``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field("b", LongIdent("'other")) ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .attribute(Attribute "Struct")

            }
        }

        |> produces
            """
[<Struct>]
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

"""
