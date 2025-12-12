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
    let ``Produces Mutually Recursive Records``() =
        Oak() {
            AnonymousModule() {
                Record("Person") {
                    Field("Name", String())
                    Field("Age", Int())
                    Field("Address", LongIdent("Address"))
                }

                Record("Address") {
                    Field("Line1", String())
                    Field("Line2", String())
                    Field("Occupant", LongIdent("Person"))
                }
                |> _.toRecursive()
            }
        }
        |> produces
            """
type Person =
    { Name: string
      Age: int
      Address: Address }

and Address =
    { Line1: string
      Line2: string
      Occupant: Person }
"""

    [<Fact>]
    let ``yield! multiple records``() =
        let genRecord(identifier: string) =
            Record(identifier) { Field("X", Float()) }

        let generateModel identifiers =
            Oak() {
                AnonymousModule() {
                    genRecord("R")
                    yield! identifiers |> List.map genRecord
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

    [<Fact>]
    let ``yield! multiple record fields``() =
        Oak() {
            AnonymousModule() { Record("Color") { yield! [ Field("R", Int()); Field("G", Int()); Field("B", Int()) ] } }
        }
        |> produces
            """
type Color = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces a record with private field``() =
        Oak() {
            AnonymousModule() {
                Record("Person") {
                    Field("Name", String()).toPrivate()
                    Field("Age", Int())
                }
            }
        }
        |> produces
            """
type Person = { private Name: string; Age: int }
"""

    [<Fact>]
    let ``Produces a record with internal field``() =
        Oak() {
            AnonymousModule() {
                Record("Person") {
                    Field("Name", String()).toInternal()
                    Field("Age", Int())
                }
            }
        }
        |> produces
            """
type Person = { internal Name: string; Age: int }
"""

    [<Fact>]
    let ``Produces a record with public field``() =
        Oak() {
            AnonymousModule() {
                Record("Person") {
                    Field("Name", String()).toPublic()
                    Field("Age", Int())
                }
            }
        }
        |> produces
            """
type Person = { public Name: string; Age: int }
"""
