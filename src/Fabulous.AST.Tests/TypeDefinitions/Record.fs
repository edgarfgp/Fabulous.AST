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
                    Field(colour, TypeLongIdent("int"))
            })
                .attribute("Serializable")
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
                Field("Red", TypeLongIdent("int")).attribute("Obsolete")

                Field("Green", TypeLongIdent("int"))
                Field("Blue", TypeLongIdent("int"))
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
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
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
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
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

    [<Test>]
    let ``Produces an obsolete struct generic record`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
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
