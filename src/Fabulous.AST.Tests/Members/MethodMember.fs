namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open NUnit.Framework

module MethodMembers =

    [<Test>]
    let ``Produces a record with TypeParams and method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                MethodMember("this.A", MemberParameters([ ("p", Some CommonType.String) ], false)) { Constant("\"\"") }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A (p: string) = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and static method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                StaticMethodMember("A", MemberParameters([ ("p", Some CommonType.String) ], false)) { Constant("\"\"") }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A (p: string) = ""

"""

    [<Test>]
    let ``Produces a record with method member`` () =
        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                MethodMember("this.A", MemberParameters([ ("p", Some CommonType.String) ], false)) { Constant("\"\"") }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A (p: string) = ""

"""

    [<Test>]
    let ``Produces a record with static method member`` () =
        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                StaticMethodMember("A", MemberParameters([ ("p", Some CommonType.String) ], false)) { Constant("\"\"") }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A (p: string) = ""

"""


    [<Test>]
    let ``Produces a classes with a method member`` () =
        AnonymousModule() {
            (Class("Person") { MethodMember("this.Name()", Parameters([], false)) { Constant("23") } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Test>]
    let ``Produces a classes with a method member and parameter`` () =
        AnonymousModule() {
            (Class("Person") {
                MethodMember("this.Name", MemberParameters([ ("params", Some CommonType.String) ], false)) {
                    Constant("23")
                }
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name (params: string) = 23
"""

    [<Ignore("FIXME: Fix extra outer parens")>]
    let ``Produces a method member with tupled parameter`` () =
        AnonymousModule() {
            (Class("Person") {
                MethodMember(
                    "this.Name",
                    MemberParameters([ ("name", Some CommonType.String); ("age", Some CommonType.Int32) ], false)
                ) {
                    Constant("23")
                }

            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Ignore("FIXME: Fix extra outer parens")>]
    let ``Produces a method member with multiple parameter`` () =
        AnonymousModule() {
            (Class("Person") {
                MethodMember(
                    "this.Name",
                    MemberParameters([ ("name", Some CommonType.String); ("age", Some CommonType.Int32) ], false)
                ) {
                    Constant("23")
                }
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Test>]
    let ``Produces a method member with attributes`` () =
        AnonymousModule() {
            (Class("Person") {
                (MethodMember("this.Name()", MemberParameters([], false)) { Constant("23") })
                    .attributes([ "Obsolete" ])
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name() = 23

"""

    [<Test>]
    let ``Produces an inline method member`` () =
        AnonymousModule() {
            (Class("Person") { (MethodMember("this.Name()") { Constant("23") }).isInlined() })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Ignore("FIXME: Fix extra outer parens")>]
    let ``Produces an method member with type parameters`` () =
        AnonymousModule() {
            (Class("Person") {
                (MethodMember("this.Name()") { Constant("23") })
                    .genericTypeParameters([ "'other" ])
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Test>]
    let ``Produces a union with a method member `` () =
        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() { MethodMember("this.Name()") { Constant("\"name\"") } }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name() = "name"

"""

    [<Test>]
    let ``Produces a generic union with a method member`` () =
        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParameterizedCase("Red") {
                    Field("a", CommonType.String)
                    Field("b", CommonType.mkLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                MethodMember("this.Name()") { Constant("\"name\"") }
            }

        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name() = "name"

"""
