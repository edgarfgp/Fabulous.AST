namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

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
    let ``Produces an union with interface member``() =
        Oak() {
            AnonymousModule() {

                Interface("IMyInterface") {
                    let parameters = [ Unit() ]
                    AbstractCurriedMethod("GetValue", parameters, String())
                }

                (Union("Colors") {
                    UnionCase("Red")
                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .members() {
                    InterfaceMember("IMyInterface") {
                        Property(ConstantPat(Constant("x.GetValue")), ConstantExpr(String ""))
                    }
                }
            }
        }
        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

type Colors =
    | Red
    | Green
    | Blue
    | Yellow

    interface IMyInterface with
        member x.GetValue = ""

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
                Interface("IMyInterface") { AbstractCurriedMethod("GetValue", [ Unit() ], String()) }

                (Union("Colors") {
                    UnionCase("Red", [ Field("a", String()); Field(LongIdent("'other")) ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    InterfaceMember(LongIdent "IMyInterface") {
                        Property(ConstantPat(Constant("x.GetValue")), ConstantExpr(String ""))
                    }
                }
            }
        }

        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

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
