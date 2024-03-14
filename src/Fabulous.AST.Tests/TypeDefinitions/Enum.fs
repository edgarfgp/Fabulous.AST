namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Fabulous.AST.Ast

module Enum =

    [<Fact>]
    let ``Produces an enum``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", "0").hasQuotes(false)
                    EnumCase("Green", "1").hasQuotes(false)
                    EnumCase("Blue", "2").hasQuotes(false)
                }
            }
        }

        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
"""

    [<Fact>]
    let ``Produces an enum with value Expr``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", "0").hasQuotes(false)
                    EnumCase("Green", ConstantExpr("1").hasQuotes(false))
                    EnumCase("Blue", "2").hasQuotes(false)
                }
            }
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
"""

    [<Fact>]
    let ``Produces an enum with SingleTextNode``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", "0").hasQuotes(false)
                    EnumCase("Green", "1").hasQuotes(false)
                    EnumCase("Blue", "2").hasQuotes(false)
                }
            }
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2

"""

    [<Fact>]
    let ``Produces an enum case using EnumCaseNode``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", "0").hasQuotes(false)
                    EnumCase("Green", "1").hasQuotes(false)
                    EnumCase("Blue", "2").hasQuotes(false)

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
        }
        |> produces
            """

type Colors =
    | Red = 0
    | Green = 1
    | Blue = 2
    | Black = 3

"""

    [<Fact>]
    let ``Produces an enum case using Widget and escape hatch``() =
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

        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", "0").hasQuotes(false)
                    EnumCase("Green", "1").hasQuotes(false)
                    EnumCase("Blue", "2").hasQuotes(false)
                    EscapeHatch(enumCaseNode)
                }
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

    [<Fact>]
    let ``Produces an enum case for a list``() =
        let colors = [ "Red"; "Green"; "Blue"; "Black" ]

        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    for i = 0 to colors.Length - 1 do
                        EnumCase(colors.[i], $"{i}").hasQuotes(false)
                }

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

    [<Fact>]
    let ``Produces an enum with attribute``() =
        let colors = [ "Red"; "Green"; "Blue"; "Black" ]

        Oak() {
            AnonymousModule() {
                (Enum("Colors") {
                    for i = 0 to colors.Length - 1 do
                        EnumCase(colors.[i], $"{i}").hasQuotes(false)
                })
                    .attribute("FlagsAttribute")

            }
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

    [<Fact>]
    let ``Produces an enum case with attributes``() =
        Oak() {
            AnonymousModule() {
                (Enum("Colors") {
                    EnumCase("Red", "0").attributes([ "Obsolete"; "MyAttribute" ]).hasQuotes(false)

                    EnumCase("Green", "1").hasQuotes(false)
                    EnumCase("Blue", "2").hasQuotes(false)
                })
                    .attribute("FlagsAttribute")
            }
        }
        |> produces
            """
[<FlagsAttribute>]
type Colors =
    | [<Obsolete; MyAttribute>] Red = 0
    | Green = 1
    | Blue = 2

"""
