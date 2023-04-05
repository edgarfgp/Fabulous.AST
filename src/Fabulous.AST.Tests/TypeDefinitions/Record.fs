namespace Fabulous.AST.Tests.TypeDefinitions

open FSharp.Compiler.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Record =
    [<Test>]
    let ``Produces a record`` () =

        AnonymousModule() {
            Record("Colors") {
                Field("Red", Type.FromString "int")
                Field("Green", Type.FromString "int")
                Field("Blue", Type.FromString "int")
            }
        }
        |> produces
            """

type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Test>]
    let ``Produces a record using EscapeHatch`` () =
        let customField =
            FieldNode(
                Some(XmlDocNode([| "/// Super cool doc bro" |], Range.Zero)),
                None,
                None,
                false,
                None,
                Some(SingleTextNode("Green", Range.Zero)),
                Type.FromString "int",
                Range.Zero
            )

        AnonymousModule() {
            Record("Colors") {
                Field("Red", Type.FromString "int")
                EscapeHatch(customField)
                Field("Blue", Type.FromString "int")
            }
        }
        |> produces
            """

type Colors =
    {
        Red: int
        /// Super cool doc bro
        Green: int
        Blue: int
    }

"""

    [<Test>]
    let ``Produces a record using a loop`` () =
        AnonymousModule() {
            Record("Colors") {
                for colour in [ "Red"; "Green"; "Blue" ] do
                    Field(colour, Type.FromString "int")
            }
        }
        |> produces
            """

type Colors = { Red: int; Green: int; Blue: int }

"""
