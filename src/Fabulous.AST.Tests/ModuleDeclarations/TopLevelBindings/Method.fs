namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module MethodMembers =

    [<Fact>]
    let ``Produces MethodMembers``() =

        AnonymousModule() {
            (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                Method("this.A", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))

                Method("this.C", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toInlined()

                Method("B", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toStatic()

                Method("D", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toInlined()
                    .toStatic()

                Method("this.E", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                |> _.returnType(String())

                Method("this.F", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toInlined()
                |> _.returnType(String())

                Method("G", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toStatic()
                |> _.returnType(String())

                Method("H", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toStatic()
                    .toInlined()
                |> _.returnType(String())

                Method("this.I", ParametersPat(true) { ParameterPat("p", String()) }, ConstantExpr(""))

                Method(
                    "this.J",
                    ParametersPat(true) {
                        ParameterPat("p", String())
                        ParameterPat("p2", String())
                    },
                    ConstantExpr("")
                )

                Method(
                    "this.K",
                    ParametersPat() {
                        ParameterPat("p", String())
                        ParameterPat("p2", String())
                    },
                    ConstantExpr("")
                )

                Method(
                    "__.DoSomething",
                    UnitPat(),
                    IfThenElseExpr(
                        InfixAppExpr(
                            ConstantExpr(Constant("x").hasQuotes(false)),
                            "=",
                            ConstantExpr(Constant("12").hasQuotes(false))
                        ),
                        ConstantExpr(ConstantUnit()),
                        ConstantExpr(ConstantUnit())
                    )

                )
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A (p: string) = ""
    member inline this.C (p: string) = ""
    static member B (p: string) = ""
    static member inline D (p: string) = ""
    member this.E (p: string) : string = ""
    member inline this.F (p: string) : string = ""
    static member G (p: string) : string = ""
    static member inline H (p: string) : string = ""
    member this.I(p: string) = ""
    member this.J(p: string, p2: string) = ""
    member this.K (p: string) (p2: string) = ""
    member __.DoSomething() = if x = 12 then () else ()

"""

    [<Fact>]
    let ``Produces a record with TypeParams and method member``() =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            })
                .members() {
                Method("this.A", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
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

    [<Fact>]
    let ``Produces a record with TypeParams and static method member``() =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            })
                .members() {
                Method("A", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toStatic()
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

    [<Fact>]
    let ``Produces a record with method member``() =
        AnonymousModule() {
            (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                Method("this.A", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A (p: string) = ""

"""

    [<Fact>]
    let ``Produces a record with static method member``() =
        AnonymousModule() {
            (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                Method("A", ParametersPat() { ParameterPat("p", String()) }, ConstantExpr(""))
                    .toStatic()
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A (p: string) = ""

"""

    [<Fact>]
    let ``Produces a classes with a method member``() =
        AnonymousModule() { Class("Person") { Method("this.Name", UnitPat(), ConstantExpr("23").hasQuotes(false)) } }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Fact>]
    let ``Produces a classes with a method member and parameter``() =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat() { ParameterPat("params", String()) },
                    ConstantExpr("23").hasQuotes(false)
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name (params: string) = 23
"""

    [<Fact>]
    let ``Produces a method member with tupled parameter``() =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat(true) {
                        ParameterPat("name", String())
                        ParameterPat("age", Int32())
                    },
                    ConstantExpr("23").hasQuotes(false)
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Fact>]
    let ``Produces a method member with multiple parameter``() =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat() {
                        ParameterPat("name", String())
                        ParameterPat("age", Int32())
                    },
                    ConstantExpr("23").hasQuotes(false)
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Fact>]
    let ``Produces a method member with attributes``() =
        AnonymousModule() {
            Class("Person") {
                Method("this.Name", UnitPat(), ConstantExpr("23").hasQuotes(false))
                    .attribute("Obsolete")
            }
        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name() = 23

"""

    [<Fact>]
    let ``Produces an inline method member``() =
        AnonymousModule() {
            Class("Person") { Method("this.Name", UnitPat(), ConstantExpr("23").hasQuotes(false)).toInlined() }
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Fact>]
    let ``Produces an method member with type parameters``() =
        AnonymousModule() {
            Class("Person") {
                Method("this.Name", UnitPat(), ConstantExpr("23").hasQuotes(false))
                    .typeParameters([ "'other" ])
            }
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Fact>]
    let ``Produces a union with a method member ``() =
        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() { Method("this.Name", UnitPat(), ConstantExpr("name")) }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name() = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a method member``() =
        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParamsCase("Red") {
                    Field("a", LongIdent("string"))
                    Field("b", LongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                Method("this.Name", UnitPat(), ConstantExpr("name"))
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
