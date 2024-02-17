namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST
open type Ast

module Record =

    [<Test>]
    let ``Produces a record with an attribute`` () =
        AnonymousModule() {
            (Record("Colors") {
                for colour in [ "Red"; "Green"; "Blue" ] do
                    Field(colour, CommonType.Int32)
            })
                .attributes(AttributeNode "Serializable")
        }
        |> produces
            """

[<Serializable>]
type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Test>]
    let ``Produces a record field with an attribute`` () =
        AnonymousModule() {
            Record("Colors") {
                Field("Red", CommonType.Int32)
                    .attributes(AttributeNode "Obsolete")

                Field("Green", CommonType.Int32)
                Field("Blue", CommonType.Int32)
            }
        }
        |> produces
            """

type Colors =
    { [<Obsolete>]
      Red: int
      Green: int
      Blue: int }

"""

    [<Test>]
    let ``Produces a generic record`` () =
        AnonymousModule() {
            GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""

    [<Test>]
    let ``Produces a struct generic record`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .attributes(AttributeNode "Struct")
        }

        |> produces
            """
[<Struct>]
type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""

    [<Test>]
    let ``Produces an obsolete struct generic record`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .attributes(
                    AttributeNodes() {
                        AttributeNode "Struct"
                        AttributeNode "Obsolete"
                    }
                )
        }

        |> produces
            """
[<Struct; Obsolete>]
type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""
