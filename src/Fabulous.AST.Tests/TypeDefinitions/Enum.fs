namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module Enum =

    [<Test>]
    let ``Produces an enum`` () =
        AnonymousModule() {
            Enum("Colors") {
                EnumCase("Red", "0")
                EnumCase("Green", "1")
                EnumCase("Blue", "2")
            }
        }

        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
"""

    [<Test>]
    let ``Produces an enum with value Expr`` () =
        AnonymousModule() {
            Enum("Colors") {
                EnumCase("Red", "0")
                EnumCase("Green", ConstantExpr("1"))
                EnumCase("Blue", "2")
            }
        }

        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
"""

    [<Test>]
    let ``Produces an enum with SingleTextNode`` () =
        AnonymousModule() {
            Enum("Colors") {
                EnumCase("Red", "0")
                EnumCase("Green", "1")
                EnumCase("Blue", "2")
            }
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2

"""

    [<Test>]
    let ``Produces an enum case using EnumCaseNode`` () =
        AnonymousModule() {
            Enum("Colors") {
                EnumCase("Red", "0")
                EnumCase("Green", "1")
                EnumCase("Blue", "2")

                EnumCaseNode(
                    None,
                    None,
                    None,
                    SingleTextNode("Black", Range.Zero),
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("3", Range.Zero))),
                    Range.Zero
                )
            }
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Black = 3

"""

    [<Test>]
    let ``Produces an enum case using Widget and escape hatch`` () =
        let enumCaseNode =
            EnumCaseNode(
                None,
                None,
                None,
                SingleTextNode("Black", Range.Zero),
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(SingleTextNode("3", Range.Zero))),
                Range.Zero
            )

        AnonymousModule() {
            Enum("Colors") {
                EnumCase("Red", "0")
                EnumCase("Green", "1")
                EnumCase("Blue", "2")
                EscapeHatch(enumCaseNode)
            }
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Black = 3

"""

    [<Test>]
    let ``Produces an enum case for a list`` () =
        let colors = [ "Red"; "Green"; "Blue"; "Black" ]

        AnonymousModule() {
            Enum("Colors") {
                for i = 0 to colors.Length - 1 do
                    EnumCase(colors.[i], $"{i}")
            }

        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Black = 3

"""


    [<Test>]
    let ``Produces an enum with attribute`` () =
        let colors = [ "Red"; "Green"; "Blue"; "Black" ]

        AnonymousModule() {
            (Enum("Colors") {
                for i = 0 to colors.Length - 1 do
                    EnumCase(colors.[i], $"{i}")
            })
                .attribute("FlagsAttribute")

        }
        |> produces
            """

[<FlagsAttribute>]
type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Black = 3

"""

    [<Test>]
    let ``Produces an enum case with attributes`` () =
        AnonymousModule() {
            (Enum("Colors") {
                EnumCase("Red", "0")
                    .attributes([ "Obsolete"; "MyAttribute" ])

                EnumCase("Green", "1")
                EnumCase("Blue", "2")
            })
                .attribute("FlagsAttribute")
        }
        |> produces
            """
[<FlagsAttribute>]
type Colors =
    | [<Obsolete; MyAttribute>] Red = 0
    | Green = 1
    | Blue = 2

"""
