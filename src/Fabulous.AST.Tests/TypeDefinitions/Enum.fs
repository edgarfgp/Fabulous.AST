namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit

open type Fabulous.AST.Ast

module Enum =

    [<Fact>]
    let ``Produces an enum``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", ConstantExpr(Int 0))
                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))
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
                    EnumCase("Red", ConstantExpr(Int 0))
                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))
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
    let ``Produces an enum with value Constant``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", Int(0))
                    EnumCase("Green", Int(1))
                    EnumCase("Blue", Int(2))
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
    let ``Produces an enum with field``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    EnumCase("Red", Int(0))
                    EnumCase("Green", Int(1))
                    EnumCase("Blue", Int(2))
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
                    EnumCase("Red", ConstantExpr(Int 0))
                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))
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
                    EnumCase("Red", ConstantExpr(Int 0))
                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))

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
                    EnumCase("Red", ConstantExpr(Int 0))
                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))
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
                        EnumCase(colors.[i], ConstantExpr(Constant $"{i}"))
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
                        EnumCase(colors.[i], ConstantExpr(Constant $"{i}"))
                })
                    .attribute(Attribute "FlagsAttribute")

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
                    EnumCase("Red", ConstantExpr(Int 0)).attributes([ Attribute "Obsolete"; Attribute "MyAttribute" ])

                    EnumCase("Green", ConstantExpr(Int 1))
                    EnumCase("Blue", ConstantExpr(Int 2))
                })
                    .attribute(Attribute "FlagsAttribute")
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

    [<Fact>]
    let ``yield! multiple enums``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ Enum("Colors") {
                          EnumCase("Red", Int(0))
                          EnumCase("Green", Int(1))
                      }
                      Enum("Sizes") {
                          EnumCase("Small", Int(0))
                          EnumCase("Large", Int(1))
                      } ]
            }
        }
        |> produces
            """
type Colors =
    | Red = 0
    | Green = 1

type Sizes =
    | Small = 0
    | Large = 1
"""

    [<Fact>]
    let ``yield! multiple enum cases``() =
        Oak() {
            AnonymousModule() {
                Enum("Colors") {
                    yield! [ EnumCase("Red", Int(0)); EnumCase("Green", Int(1)); EnumCase("Blue", Int(2)) ]
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
