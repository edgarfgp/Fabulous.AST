namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open NUnit.Framework

module PropertyMember =

    [<Test>]
    let ``Produces PropertiesMembers`` () =

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                Member("this.A", ConstantExpr(ConstantString "\"\""))
                InlinedMember("this.C", ConstantExpr(ConstantString "\"\""))

                StaticMember("B", ConstantExpr(ConstantString "\"\""))
                InlinedStaticMember("D", ConstantExpr(ConstantString "\"\""))

                Member("this.E", ConstantExpr(ConstantString "\"\""))
                |> _.returnType(CommonType.String)

                InlinedMember("this.F", ConstantExpr(ConstantString "\"\""))
                |> _.returnType(CommonType.String)

                StaticMember("G", ConstantExpr(ConstantString "\"\""))
                |> _.returnType(CommonType.String)

                InlinedStaticMember("H", ConstantExpr(ConstantString "\"\""))
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
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                Member(NamedPat("this.A"), ConstantExpr(ConstantString("\"\"")))
                InlinedMember("this.C", ConstantExpr(ConstantString("\"\"")))

                StaticMember(ParenPat(NamedPat("|B|_|")), ConstantExpr(ConstantString("\"\"")))
                InlinedStaticMember("D", ConstantExpr(ConstantString("\"\"")))

                Member("this.E", ConstantExpr(ConstantString("\"\"")))
                |> _.returnType(CommonType.String)

                InlinedMember("this.F", ConstantExpr(ConstantString("\"\"")))
                |> _.returnType(CommonType.String)

                StaticMember("G", ConstantExpr(ConstantString("\"\"")))
                |> _.returnType(CommonType.String)

                InlinedStaticMember(NamedPat("H"), ConstantExpr(ConstantString("\"\"")))
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
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                Member("this.A", ConstantExpr(ConstantString("\"\"")))
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
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                StaticMember("A", ConstantExpr(ConstantString("\"\"")))
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
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                Member("this.A", ConstantExpr(ConstantString("\"\"")))
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
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                StaticMember("A", ConstantExpr(ConstantString("\"\"")))
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
                Member("this.Name1", ConstantExpr(ConstantString("\"name\"")))

                StaticMember("Name2", ConstantExpr(ConstantString("\"name\"")))
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
                Member("this.Name1", ConstantExpr(ConstantString("\"name\"")))

                StaticMember("Name2", ConstantExpr(ConstantString("\"name\"")))
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
                (Member("this.Name", ConstantExpr(ConstantString("\"name\""))))
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
                    let widget = Member($"this.{name}", ConstantExpr(ConstantString("\"name\"")))

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
            Class("Person") { Member("this.Name", ConstantExpr(ConstantString("23"))) |> _.returnType("int") }

        }
        |> produces
            """
type Person () =
    member this.Name: int = 23
"""

    [<Test>]
    let ``Produces a class with a member property inlined`` () =
        AnonymousModule() { Class("Person") { InlinedMember("this.Name", ConstantExpr(ConstantString("\"name\""))) } }
        |> produces
            """
type Person () =
    member inline this.Name = "name"

"""

    [<Test>]
    let ``Produces a class with property member with attributes`` () =
        AnonymousModule() {
            Class("Person") {
                (Member("this.Name", ConstantExpr(ConstantString "23")))
                    .attributes(AttributeNode "Obsolete")
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
            (Record("Person") { Field("Name", CommonType.String) })
                .members() {
                Member("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
            (GenericRecord("Person", [ "'other" ]) { Field("Name", CommonType.mkLongIdent("'other")) })
                .members() {
                Member("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                Member("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                StaticMember("Name", ConstantExpr(ConstantString("\"name\"")))
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
                    Field("a", CommonType.String)
                    Field("b", CommonType.mkLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                Member("this.Name", ConstantExpr(ConstantString("\"name\"")))
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
                    Field("a", CommonType.String)
                    Field("b", CommonType.mkLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                StaticMember("Name", ConstantExpr(ConstantString("\"name\"")))
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
