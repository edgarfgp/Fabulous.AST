namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module PropertyMember =

    [<Fact>]
    let ``Produces PropertiesMembers``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", "string") })
                    .members(
                        [ Property("this.A", ConstantExpr(DoubleQuoted ""))

                          Property("this.C", ConstantExpr(DoubleQuoted "")).toInlined()

                          Property("B", ConstantExpr(DoubleQuoted "")).toStatic()

                          Property("D", ConstantExpr(DoubleQuoted "")).toStatic().toInlined()

                          Property("this.E", ConstantExpr(DoubleQuoted "")) |> _.returnType(String())

                          Property("this.F", ConstantExpr(DoubleQuoted "")).toInlined()
                          |> _.returnType(String())

                          Property("G", ConstantExpr(DoubleQuoted "")).toStatic()
                          |> _.returnType(String())

                          Property("H", ConstantExpr(DoubleQuoted "")).toStatic().toInlined()
                          |> _.returnType(String()) ]
                    )
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""
    member inline this.C = ""
    static member B = ""
    static member inline D = ""
    member this.E: string = ""
    member inline this.F: string = ""
    static member G: string = ""
    static member inline H: string = ""

"""

    [<Fact>]
    let ``Produces Properties with Patterns``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Property("this.A", ConstantExpr(DoubleQuoted ""))

                          Property("this.C", ConstantExpr(DoubleQuoted "")).toInlined()

                          Property("(|B|_|)", ConstantExpr(DoubleQuoted "")).toStatic()

                          Property("D", ConstantExpr(DoubleQuoted "")).toStatic().toInlined()

                          Property("this.E", ConstantExpr(DoubleQuoted "")) |> _.returnType(String())

                          Property("this.F", ConstantExpr(DoubleQuoted "")).toInlined()
                          |> _.returnType(String())

                          Property("G", ConstantExpr(DoubleQuoted "")).toStatic()
                          |> _.returnType(String())

                          Property("H", ConstantExpr(DoubleQuoted "")).toStatic().toInlined()
                          |> _.returnType(String()) ]
                    )
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""
    member inline this.C = ""
    static member (|B|_|) = ""
    static member inline D = ""
    member this.E: string = ""
    member inline this.F: string = ""
    static member G: string = ""
    static member inline H: string = ""

"""

    [<Fact>]
    let ``Produces a record with property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members([ Property("this.A", ConstantExpr(DoubleQuoted "")) ])
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""

"""

    [<Fact>]
    let ``Produces a record with static property member``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members([ Property("A", ConstantExpr(DoubleQuoted "")).toStatic() ])
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A = ""

"""

    [<Fact>]
    let ``Produces a record with TypeParams and property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members([ Property("this.A", ConstantExpr(DoubleQuoted "")) ])
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A = ""

"""

    [<Fact>]
    let ``Produces a record with TypeParams and static property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members([ Property("A", ConstantExpr(DoubleQuoted "")).toStatic() ])
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A = ""

"""

    [<Fact>]
    let ``Produces a class with a static and not static member property ``() =

        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Property("this.Name1", ConstantExpr(DoubleQuoted "name"))

                    Property("Name2", ConstantExpr(DoubleQuoted "name")).toStatic()
                }
            }
        }
        |> produces
            """
type Person() =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Fact>]
    let ``Produces a generic class with a static and not static member property ``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Property("this.Name1", ConstantExpr(DoubleQuoted "name"))

                    Property("Name2", ConstantExpr(DoubleQuoted "name")).toStatic()
                }
                |> _.typeParams([ "'other" ])
            }
        }
        |> produces
            """
type Person<'other>() =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property with xml comments``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Property("this.Name", ConstantExpr(DoubleQuoted "name"))
                        .xmlDocs([ "This is a comment" ])
                }
            }
        }
        |> produces
            """
type Person() =
    /// This is a comment
    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property and accessibility controls``() =
        let data =
            [ "Name", AccessControl.Public
              "Age", AccessControl.Private
              "Address", AccessControl.Internal
              "PostalCode", AccessControl.Unknown ]

        Oak() {
            AnonymousModule() {
                Class("Person") {
                    for name, acc in data do
                        let widget = Property($"this.{name}", ConstantExpr(DoubleQuoted "name"))

                        match acc with
                        | AccessControl.Public -> widget.toPublic()
                        | AccessControl.Private -> widget.toPrivate()
                        | AccessControl.Internal -> widget.toInternal()
                        | AccessControl.Unknown -> widget
                }
            }
        }
        |> produces
            """
type Person() =
    member public this.Name = "name"
    member private this.Age = "name"
    member internal this.Address = "name"
    member this.PostalCode = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property and return type``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(Unquoted "23")) |> _.returnType("int") }

            }
        }
        |> produces
            """
type Person() =
    member this.Name: int = 23
"""

    [<Fact>]
    let ``Produces a class with a member property inlined``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(DoubleQuoted "name")).toInlined() }
            }
        }
        |> produces
            """
type Person() =
    member inline this.Name = "name"

"""

    [<Fact>]
    let ``Produces a class with property member with attributes``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(Unquoted "23")).attribute("Obsolete") }

            }
        }
        |> produces
            """
type Person() =
    [<Obsolete>]
    member this.Name = 23
"""

    [<Fact>]
    let ``Produces a record with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("string")) })
                    .members([ Property("this.Name", ConstantExpr(DoubleQuoted "name")) ])

            }
        }
        |> produces
            """
type Person =
    { Name: string }

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a generic record with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("'other")) })
                    .typeParams([ "'other" ])
                    .members([ Property("this.Name", ConstantExpr(DoubleQuoted "name")) ])

            }
        }
        |> produces
            """
type Person<'other> =
    { Name: 'other }

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a union with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") })
                    .members([ Property("this.Name", ConstantExpr(DoubleQuoted "name")) ])

            }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a union with a static member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") })
                    .members([ Property("Name", ConstantExpr(DoubleQuoted "name")).toStatic() ])

            }
        }
        |> produces
            """
type Person =
    | Name

    static member Name = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams([ "'other" ])
                    .members([ Property("this.Name", ConstantExpr(DoubleQuoted "name")) ])

            }
        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a static member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams([ "'other" ])
                    .members([ Property("Name", ConstantExpr(DoubleQuoted "name")).toStatic() ])

            }
        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    static member Name = "name"

"""
