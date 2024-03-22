namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module MethodMembers =

    [<Fact>]
    let ``Produces MethodMembers``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Method("this.A", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))

                          Method("this.C", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toInlined()

                          Method("B", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toStatic()

                          Method("D", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toInlined()
                              .toStatic()

                          Method("this.E", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                          |> _.returnType(String())

                          Method("this.F", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toInlined()
                          |> _.returnType(String())

                          Method("G", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toStatic()
                          |> _.returnType(String())

                          Method("H", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toStatic()
                              .toInlined()
                          |> _.returnType(String())

                          Method(
                              "this.I",
                              ParametersPat([ ParameterPat("p", String()) ], true),
                              ConstantExpr(Quoted "")
                          )

                          Method(
                              "this.J",
                              ParametersPat([ ParameterPat("p", String()); ParameterPat("p2", String()) ], true),
                              ConstantExpr(Quoted "")
                          )

                          Method(
                              "this.K",
                              ParametersPat([ ParameterPat("p", String()); ParameterPat("p2", String()) ]),
                              ConstantExpr(Quoted "")
                          )

                          Method(
                              "__.DoSomething",
                              UnitPat(),
                              IfThenElseExpr(
                                  InfixAppExpr(
                                      ConstantExpr(Constant(Unquoted "x")),
                                      "=",
                                      ConstantExpr(Constant(Unquoted "12"))
                                  ),
                                  ConstantExpr(ConstantUnit()),
                                  ConstantExpr(ConstantUnit())
                              )

                          ) ]
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
        Oak() {
            AnonymousModule() {
                let foo = async { return 1 }

                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members(
                        [ Method("this.A", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted "")) ]
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

    [<Fact>]
    let ``Produces a record with TypeParams and static method member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members(
                        [ Method("A", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toStatic() ]
                    )
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
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Method("this.A", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted "")) ]
                    )
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
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Method("A", ParametersPat([ ParameterPat("p", String()) ]), ConstantExpr(Quoted ""))
                              .toStatic() ]
                    )
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
        Oak() {
            AnonymousModule() {
                Class("Person", Constructor()) { Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23")) }
            }
        }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Fact>]
    let ``Produces a classes with a method member and parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "this.Name",
                        ParametersPat([ ParameterPat("params", String()) ]),
                        ConstantExpr(Unquoted "23")
                    )
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name (params: string) = 23
"""

    [<Fact>]
    let ``Produces a method member with tupled parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "this.Name",
                        ParametersPat([ ParameterPat("name", String()); ParameterPat("age", Int32()) ], true),
                        ConstantExpr(Unquoted "23")
                    )
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Fact>]
    let ``Produces a method member with multiple parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "this.Name",
                        ParametersPat([ ParameterPat("name", String()); ParameterPat("age", Int32()) ]),
                        ConstantExpr(Unquoted "23")
                    )
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Fact>]
    let ``Produces a method member with attributes``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23"))
                        .attribute("Obsolete")
                }
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
        Oak() {
            AnonymousModule() {
                Class("Person") { Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23")).toInlined() }
            }
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Fact>]
    let ``Produces an method member with type parameters``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23"))
                        .typeParams([ "'other" ])
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Fact>]
    let ``Produces a union with a method member ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") })
                    .members([ Method("this.Name", UnitPat(), ConstantExpr(Quoted "name")) ])
            }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name() = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a method member``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionParamsCase("Red") {
                        Field("a", LongIdent("string"))
                        Field("b", LongIdent "'other")
                    }

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams([ "'other" ])
                    .members([ Method("this.Name", UnitPat(), ConstantExpr(Quoted "name")) ])

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
