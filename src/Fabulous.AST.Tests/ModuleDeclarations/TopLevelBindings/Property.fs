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
                Property("this.A", ConstantExpr(ConstantString "\"\""))

                Property("this.C", ConstantExpr(ConstantString "\"\""))
                    .toInlined()

                Property("B", ConstantExpr(ConstantString "\"\""))
                    .toStatic()

                Property("D", ConstantExpr(ConstantString "\"\""))
                    .toStatic()
                    .toInlined()

                Property("this.E", ConstantExpr(ConstantString "\"\""))
                |> _.returnType(CommonType.String)

                Property("this.F", ConstantExpr(ConstantString "\"\""))
                    .toInlined()
                |> _.returnType(CommonType.String)

                Property("G", ConstantExpr(ConstantString "\"\""))
                    .toStatic()
                |> _.returnType(CommonType.String)

                Property("H", ConstantExpr(ConstantString "\"\""))
                    .toStatic()
                    .toInlined()
                |> _.returnType(CommonType.String)
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
                Property("this.A", ConstantExpr(ConstantString("\"\"")))

                Property("this.C", ConstantExpr(ConstantString("\"\"")))
                    .toInlined()

                Property("(|B|_|)", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()

                Property("D", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()
                    .toInlined()

                Property("this.E", ConstantExpr(ConstantString("\"\"")))
                |> _.returnType(CommonType.String)

                Property("this.F", ConstantExpr(ConstantString("\"\"")))
                    .toInlined()
                |> _.returnType(CommonType.String)

                Property("G", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()
                |> _.returnType(CommonType.String)

                Property("H", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()
                    .toInlined()
                |> _.returnType(CommonType.String)
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
                Property("this.A", ConstantExpr(ConstantString("\"\"")))
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
                Property("A", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()
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
                Property("this.A", ConstantExpr(ConstantString("\"\"")))
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
                Property("A", ConstantExpr(ConstantString("\"\"")))
                    .toStatic()
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
                Property("this.Name1", ConstantExpr(ConstantString("\"name\"")))

                Property("Name2", ConstantExpr(ConstantString("\"name\"")))
                    .toStatic()
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
                Property("this.Name1", ConstantExpr(ConstantString("\"name\"")))

                Property("Name2", ConstantExpr(ConstantString("\"name\"")))
                    .toStatic()
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
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                    let widget = Property($"this.{name}", ConstantExpr(ConstantString("\"name\"")))

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
            Class("Person") { Property("this.Name", ConstantExpr(ConstantString("23"))) |> _.returnType("int") }

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
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                Property("this.Name", ConstantExpr(ConstantString "23"))
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
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
            (Union("Person") { UnionCase("Name") }).members() {
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
            }

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
                Property("Name", ConstantExpr(ConstantString("\"name\"")))
                    .toStatic()
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
                Property("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                Property("Name", ConstantExpr(ConstantString("\"name\"")))
                    .toStatic()
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
