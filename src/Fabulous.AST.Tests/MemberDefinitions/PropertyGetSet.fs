namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module PropertyGetSet =

    [<Fact>]
    let ``Produces a classes with get set members``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Object3D", UnitPat()) { Member("this.Position", Getter(UnitExpr()), Setter(UnitExpr())) }

                TypeDefn("Object3D", UnitPat()) { Member("this.Position", Setter(Constant("()"))) }

                TypeDefn("Object3D", UnitPat()) {
                    Member(
                        "this.Position",
                        Setter([ ParenPat(ParameterPat("a", Int())) ], ConstantExpr "_position <- a")
                    )
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member("this.Position", Setter([ Constant("a") ], ConstantExpr "_position <- a"))
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member("this.Position", Setter([ "a" ], ConstantExpr "_position <- a"))
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member("this.Position", Setter([ ParenPat(ParameterPat("a", Int())) ], Constant "_position <- a"))
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member("this.Position", Setter([ Constant("a") ], Constant "_position <- a"))
                }

                TypeDefn("Object3D", UnitPat()) { Member("this.Position", Setter([ "a" ], Constant "_position <- a")) }

                TypeDefn("Object3D", UnitPat()) {
                    Member(
                        "this.Position",
                        Setter(
                            ParenPat("index"),
                            IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")))
                        )
                    )
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member(
                        "this.Position",
                        Setter(
                            Constant("index"),
                            IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")))
                        )
                    )
                }

                TypeDefn("Object3D", UnitPat()) {
                    Member(
                        "this.Position",
                        Setter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index"))))
                    )
                }

                TypeDefn(
                    "Person",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("name")), String())
                              ParameterPat(ConstantPat(Constant("age")), Int()) ]
                        )
                    )
                ) {
                    Member(
                        "this.Name",
                        Getter(ConstantExpr(Constant("name"))),
                        Setter(NamedPat("value"), ConstantExpr(Constant("()")))
                    )

                    Member(
                        "this.Age",
                        Getter(ConstantExpr(Constant("age"))),
                        Setter(NamedPat("value"), ConstantExpr(ConstantUnit()))
                    )
                        .toInlined()

                    Member(
                        "this.Name2",
                        Getter(ConstantExpr(Constant("name"))).toInlined(),
                        Setter(NamedPat("value"), ConstantExpr(Constant("()")))
                    )
                        .toPrivate()

                    Member(
                        "this.FirstName",
                        Getter(ConstantExpr(Constant("firstName"))),
                        Setter(ParenPat(NamedPat("value")), ConstantExpr(Constant("()")))
                    )

                    Member(
                        "this.LastName",
                        Getter(ConstantExpr(Constant("lastName"))).returnType(String()),
                        Setter(
                            ParenPat(ParameterPat(ConstantPat(Constant("value")), String())),
                            ConstantExpr(ConstantUnit())
                        )
                            .returnType(Unit())
                    )

                    Member(
                        "this.Item",
                        Getter(
                            ParenPat("index"),
                            IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")))
                        ),
                        Setter(
                            [ NamedPat("index"); NamedPat("value") ],
                            SetExpr(
                                IndexWithoutDotExpr(ConstantExpr(Constant("ordinals")), Constant("index")),
                                ConstantExpr(Constant("value"))
                            )
                        )
                    )
                }

            }
        }
        |> produces
            """
type Object3D() =
    member this.Position
        with get () = ()
        and set () = ()

type Object3D() =
    member this.Position
        with set () = ()

type Object3D() =
    member this.Position
        with set (a: int) = _position <- a

type Object3D() =
    member this.Position
        with set a = _position <- a

type Object3D() =
    member this.Position
        with set a = _position <- a

type Object3D() =
    member this.Position
        with set (a: int) = _position <- a

type Object3D() =
    member this.Position
        with set a = _position <- a

type Object3D() =
    member this.Position
        with set a = _position <- a

type Object3D() =
    member this.Position
        with set (index) = ordinals[index]

type Object3D() =
    member this.Position
        with set index = ordinals[index]

type Object3D() =
    member this.Position
        with set index = ordinals[index]

type Person(name: string, age: int) =
    member this.Name
        with get () = name
        and set value = ()

    member inline this.Age
        with get () = age
        and set value = ()

    member private this.Name2
        with inline get () = name
        and set value = ()

    member this.FirstName
        with get () = firstName
        and set (value) = ()

    member this.LastName
        with get (): string = lastName
        and set (value: string): unit = ()

    member this.Item
        with get (index) = ordinals[index]
        and set index value = ordinals[index] <- value

"""
