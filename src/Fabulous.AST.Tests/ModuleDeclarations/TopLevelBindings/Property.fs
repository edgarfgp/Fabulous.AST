namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open NUnit.Framework

module PropertyMember =

    [<Test>]
    let ``Produces PropertiesMembers`` () =

        AnonymousModule() {
            (Record("Colors") { Field("X", "string") }).members() {
                Property("this.A", ConstantStringExpr(""))

                Property("this.C", ConstantStringExpr("")).toInlined()

                Property("B", ConstantStringExpr("")).toStatic()

                Property("D", ConstantStringExpr("")).toStatic().toInlined()

                Property("this.E", ConstantStringExpr("")) |> _.returnType(String())

                Property("this.F", ConstantStringExpr("")).toInlined() |> _.returnType(String())

                Property("G", ConstantStringExpr("")).toStatic() |> _.returnType(String())

                Property("H", ConstantStringExpr("")).toStatic().toInlined()
                |> _.returnType(String())
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

    [<Test>]
    let ``Produces Properties with Patterns`` () =

        AnonymousModule() {
            (Record("Colors") { Field("X", TypeLongIdent("string")) })
                .members() {
                Property("this.A", ConstantStringExpr(""))

                Property("this.C", ConstantStringExpr("")).toInlined()

                Property("(|B|_|)", ConstantStringExpr("")).toStatic()

                Property("D", ConstantStringExpr("")).toStatic().toInlined()

                Property("this.E", ConstantStringExpr("")) |> _.returnType(String())

                Property("this.F", ConstantStringExpr("")).toInlined() |> _.returnType(String())

                Property("G", ConstantStringExpr("")).toStatic() |> _.returnType(String())

                Property("H", ConstantStringExpr("")).toStatic().toInlined()
                |> _.returnType(String())
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

    [<Test>]
    let ``Produces a record with property member`` () =
        AnonymousModule() {
            (Record("Colors") { Field("X", TypeLongIdent("string")) })
                .members() {
                Property("this.A", ConstantStringExpr(""))
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""

"""

    [<Test>]
    let ``Produces a record with static property member`` () =

        AnonymousModule() {
            (Record("Colors") { Field("X", TypeLongIdent("string")) })
                .members() {
                Property("A", ConstantStringExpr("")).toStatic()
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and property member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
            })
                .members() {
                Property("this.A", ConstantStringExpr(""))
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

    [<Test>]
    let ``Produces a record with TypeParams and static property member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
            })
                .members() {
                Property("A", ConstantStringExpr("")).toStatic()
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


    [<Test>]
    let ``Produces a class with a static and not static member property `` () =

        AnonymousModule() {
            Class("Person") {
                Property("this.Name1", ConstantStringExpr("name"))

                Property("Name2", ConstantStringExpr("name")).toStatic()
            }
        }
        |> produces
            """
type Person () =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Test>]
    let ``Produces a generic class with a static and not static member property `` () =
        AnonymousModule() {
            Class("Person", [ "'other" ]) {
                Property("this.Name1", ConstantStringExpr("name"))

                Property("Name2", ConstantStringExpr("name")).toStatic()
            }
        }
        |> produces
            """
type Person <'other>() =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Test>]
    let ``Produces a class with a member property with xml comments`` () =
        AnonymousModule() {
            Class("Person") {
                Property("this.Name", ConstantStringExpr("name"))
                    .xmlDocs([ "This is a comment" ])
            }
        }
        |> produces
            """
type Person () =
    /// This is a comment
    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a class with a member property and accessibility controls`` () =
        let data =
            [ "Name", AccessControl.Public
              "Age", AccessControl.Private
              "Address", AccessControl.Internal
              "PostalCode", AccessControl.Unknown ]

        AnonymousModule() {
            Class("Person") {
                for name, acc in data do
                    let widget = Property($"this.{name}", ConstantStringExpr("name"))

                    match acc with
                    | AccessControl.Public -> widget.toPublic()
                    | AccessControl.Private -> widget.toPrivate()
                    | AccessControl.Internal -> widget.toInternal()
                    | AccessControl.Unknown -> widget
            }
        }
        |> produces
            """
type Person () =
    member public this.Name = "name"
    member private this.Age = "name"
    member internal this.Address = "name"
    member this.PostalCode = "name"
    
"""

    [<Test>]
    let ``Produces a class with a member property and return type`` () =
        AnonymousModule() {
            Class("Person") { Property("this.Name", ConstantExpr("23")) |> _.returnType("int") }

        }
        |> produces
            """
type Person () =
    member this.Name: int = 23
"""

    [<Test>]
    let ``Produces a class with a member property inlined`` () =
        AnonymousModule() {
            Class("Person") {
                Property("this.Name", ConstantStringExpr("name"))
                    .toInlined()
            }
        }
        |> produces
            """
type Person () =
    member inline this.Name = "name"

"""

    [<Test>]
    let ``Produces a class with property member with attributes`` () =
        AnonymousModule() {
            Class("Person") {
                Property("this.Name", ConstantExpr("23"))
                    .attribute("Obsolete")
            }

        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name = 23
"""

    [<Test>]
    let ``Produces a record with a member property `` () =
        AnonymousModule() {
            (Record("Person") { Field("Name", TypeLongIdent("string")) })
                .members() {
                Property("this.Name", ConstantStringExpr("name"))
            }

        }
        |> produces
            """
type Person =
    { Name: string }

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a generic record with a member property `` () =
        AnonymousModule() {
            (GenericRecord("Person", [ "'other" ]) { Field("Name", TypeLongIdent("'other")) })
                .members() {
                Property("this.Name", ConstantStringExpr("name"))
            }

        }
        |> produces
            """
type Person<'other> =
    { Name: 'other }

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a union with a member property `` () =
        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() { Property("this.Name", ConstantStringExpr("name")) }

        }
        |> produces
            """
type Person =
    | Name

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a union with a static member property `` () =
        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() {
                Property("Name", ConstantStringExpr("name")).toStatic()
            }

        }
        |> produces
            """
type Person =
    | Name

    static member Name = "name"

"""

    [<Test>]
    let ``Produces a generic union with a member property `` () =
        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParamsCase("Red") {
                    Field("a", TypeLongIdent("string"))
                    Field("b", TypeLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                Property("this.Name", ConstantStringExpr("name"))
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

    [<Test>]
    let ``Produces a generic union with a static member property `` () =
        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParamsCase("Red") {
                    Field("a", TypeLongIdent("string"))
                    Field("b", TypeLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                Property("Name", ConstantStringExpr("name")).toStatic()
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
