namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit


open Fabulous.AST
open type Ast

module Record =

    [<Theory>]
    [<InlineData("First Name", "``First Name``")>]
    [<InlineData(" First Name ", "``First Name``")>]
    [<InlineData("First_Name", "First_Name")>]
    [<InlineData("net.6", "``net.6``")>]
    [<InlineData(" net.6 ", "``net.6``")>]
    let ``Produces a record with fields with backticks`` (value: string) (expected: string) =
        AnonymousModule() { Record("Person ") { Field(value, LongIdent("int")) } }
        |> produces
            $$"""

type Person = { {{expected}}: int }

"""

    [<Fact>]
    let ``Produces a record with an attribute`` () =
        AnonymousModule() {
            (Record("Colors") {
                for colour in [ "Red"; "Green"; "Blue" ] do
                    Field(colour, LongIdent("int"))
            })
                .attribute("Serializable")
        }
        |> produces
            """

[<Serializable>]
type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Fact>]
    let ``Produces a record field with an attribute`` () =
        AnonymousModule() {
            Record("Colors") {
                Field("Red", LongIdent("int")).attribute("Obsolete")

                Field("Green", LongIdent("int"))
                Field("Blue", LongIdent("int"))
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

    [<Fact>]
    let ``Produces a generic record`` () =
        AnonymousModule() {
            GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""

    [<Fact>]
    let ``Produces a struct generic record`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            })
                .attribute("Struct")
        }

        |> produces
            """
[<Struct>]
type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""

    [<Fact>]
    let ``Produces an obsolete struct generic record`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            })
                .attributes() {
                Attribute "Struct"
                Attribute "Obsolete"
            }

        }

        |> produces
            """
[<Struct; Obsolete>]
type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""
