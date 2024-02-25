namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open NUnit.Framework

module MethodMembers =

    [<Test>]
    let ``Produces MethodMembers`` () =

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                Method(
                    "this.A",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )


                Method(
                    "this.C",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toInlined()

                Method(
                    "B",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toStatic()

                Method(
                    "D",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toInlined()
                    .toStatic()

                Method(
                    "this.E",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                |> _.returnType(CommonType.String)

                Method(
                    "this.F",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toInlined()
                |> _.returnType(CommonType.String)

                Method(
                    "G",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toStatic()
                |> _.returnType(CommonType.String)

                Method(
                    "H",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toStatic()
                    .toInlined()
                |> _.returnType(CommonType.String)

                Method(
                    "this.I",
                    ParametersPat(true) { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )

                Method(
                    "this.J",
                    ParametersPat(true) {
                        ParameterPat("p", CommonType.String)
                        ParameterPat("p2", CommonType.String)
                    },
                    ConstantExpr(ConstantString "\"\"")
                )

                Method(
                    "this.K",
                    ParametersPat() {
                        ParameterPat("p", CommonType.String)
                        ParameterPat("p2", CommonType.String)
                    },
                    ConstantExpr(ConstantString "\"\"")
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

"""

    [<Test>]
    let ``Produces a record with TypeParams and method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                Method(
                    "this.A",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
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
                Method(
                    "A",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
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

    [<Test>]
    let ``Produces a record with method member`` () =
        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                Method(
                    "this.A",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
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
                Method(
                    "A",
                    ParametersPat() { ParameterPat("p", CommonType.String) },
                    ConstantExpr(ConstantString "\"\"")
                )
                    .toStatic()
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
        AnonymousModule() { Class("Person") { Method("this.Name", UnitPat(), ConstantExpr(ConstantString "23")) } }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Test>]
    let ``Produces a classes with a method member and parameter`` () =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat() { ParameterPat("params", CommonType.String) },
                    ConstantExpr(ConstantString "23")
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name (params: string) = 23
"""

    [<Test>]
    let ``Produces a method member with tupled parameter`` () =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat(true) {
                        ParameterPat("name", CommonType.String)
                        ParameterPat("age", CommonType.Int32)
                    },
                    ConstantExpr(ConstantString "23")
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Test>]
    let ``Produces a method member with multiple parameter`` () =
        AnonymousModule() {
            Class("Person") {
                Method(
                    "this.Name",
                    ParametersPat() {
                        ParameterPat("name", CommonType.String)
                        ParameterPat("age", CommonType.Int32)
                    },
                    ConstantExpr(ConstantString "23")
                )
            }
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Test>]
    let ``Produces a method member with attributes`` () =
        AnonymousModule() {
            Class("Person") {
                Method("this.Name", UnitPat(), ConstantExpr(ConstantString "23"))
                    .attribute("Obsolete")
            }
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
            Class("Person") {
                Method("this.Name", UnitPat(), ConstantExpr(ConstantString "23"))
                    .toInlined()
            }
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Test>]
    let ``Produces an method member with type parameters`` () =
        AnonymousModule() {
            Class("Person") {
                Method("this.Name", UnitPat(), ConstantExpr(ConstantString "23"))
                    .typeParameters([ "'other" ])
            }
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Test>]
    let ``Produces a union with a method member `` () =
        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() {
                Method("this.Name", UnitPat(), ConstantExpr(ConstantString "\"name\""))
            }
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
                UnionParamsCase("Red") {
                    Field("a", CommonType.String)
                    Field("b", CommonType.mkLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                Method("this.Name", UnitPat(), ConstantExpr(ConstantString "\"name\""))
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
