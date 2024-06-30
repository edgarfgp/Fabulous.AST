namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Record =

    [<Theory>]
    [<InlineData("First Name", "``First Name``")>]
    [<InlineData(" First Name ", "`` First Name ``")>]
    [<InlineData("First_Name", "First_Name")>]
    [<InlineData("net.6", "``net.6``")>]
    [<InlineData(" net.6 ", "`` net.6 ``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces a record with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { Record("Person") { Field(value, LongIdent("int")) } } }
        |> produces
            $$"""

type Person = { {{expected}}: int }

"""

    [<Fact>]
    let ``Produces a record with an attribute``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    for colour in [ "Red"; "Green"; "Blue" ] do
                        Field(colour, LongIdent("int"))
                })
                    .attribute(Attribute "Serializable")
            }
        }
        |> produces
            """

[<Serializable>]
type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Fact>]
    let ``Produces a record field with an attribute``() =
        Oak() {
            AnonymousModule() {
                Record("Colors") {
                    Field("Red", LongIdent("int")).attribute(Attribute "Obsolete")

                    Field("Green", LongIdent("int"))
                    Field("Blue", LongIdent("int"))
                }
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
    let ``Produces a generic record``() =
        Oak() {
            AnonymousModule() {
                Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                }
                |> _.typeParams(PostfixList([ "'other" ]))
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
    let ``Produces a struct generic record``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .attribute(Attribute "Struct")
                    .typeParams(PostfixList([ "'other" ]))
            }
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
    let ``Produces an obsolete struct generic record``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .attributes([ Attribute "Struct"; Attribute "Obsolete" ])

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

    [<Fact>]
    let ``yield! multiple records``() =
        let genRecord(identifier: string) =
            Record(identifier) { Field("X", Float()) }

        let generateModel identifiers =
            Oak() {
                AnonymousModule() {
                    genRecord("R")
                    yield! identifiers |> List.map(fun ident -> AnyModuleDecl(genRecord(ident)))
                }
            }

        [ "G"; "B" ]
        |> generateModel
        |> produces
            """
type R = { X: float }
type G = { X: float }
type B = { X: float }
"""
