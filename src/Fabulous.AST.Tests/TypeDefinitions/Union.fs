namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module Union =

    [<Test>]
    let ``Produces an union`` () =
        AnonymousModule() {
            Union("Colors") {
                UnionCase("Red")
                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
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

    [<Test>]
    let ``Produces an union with fields`` () =
        AnonymousModule() {
            Union("Colors") {
                UnionParameterizedCase("Red") {
                    Field("a", "string")
                    Field("b", "int")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            }
        }

        |> produces
            """

type Colors =
    | Red of a: string * b: int
    | Green
    | Blue
    | Yellow

"""

    [<Test>]
    let ``Produces an union with SingleTextNode`` () =
        AnonymousModule() {
            Union(SingleTextNode("Colors", Range.Zero)) {
                UnionCase("Red")
                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
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

    [<Test>]
    let ``Produces an union using Widget and escape hatch`` () =
        AnonymousModule() {
            Union("Colors") {
                UnionCase("Red")
                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
                UnionCaseNode(None, None, None, SingleTextNode("Black", Range.Zero), [], Range.Zero)

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
