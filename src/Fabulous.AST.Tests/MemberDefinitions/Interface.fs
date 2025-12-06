namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Microsoft.FSharp.Core
open Xunit

open type Ast

module InterfaceMembers =
    [<Fact>]
    let ``Produces a record with TypeParams and interface member``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMyInterface") { AbstractMember("GetValue", [ Unit() ], String()) }
                TypeDefn("IMyInterface2") { AbstractMember("GetValue", [ Unit() ], String()) }

                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    InterfaceWith(LongIdent "IMyInterface") {
                        Member("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                    }

                    InterfaceWith("IMyInterface2") { () }
                }

            }
        }

        |> produces
            """
type IMyInterface =
    abstract GetValue: unit -> string

type IMyInterface2 =
    abstract GetValue: unit -> string

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    interface IMyInterface with
        member x.GetValue() = x.MyField2

    interface IMyInterface2

"""

    [<Fact>]
    let ``Produces a record with interface member``() =
        Oak() {

            AnonymousModule() {
                TypeDefn("IMyInterface") {
                    let parameters = [ Unit() ]
                    AbstractMember("GetValue", parameters, String())
                }

                (Record("MyRecord") {
                    Field("MyField1", LongIdent("int"))
                    Field("MyField2", LongIdent("string"))
                })
                    .members() {
                    InterfaceWith(LongIdent "IMyInterface") {
                        Member("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                    }
                }
            }
        }
        |> produces
            """

type IMyInterface =
    abstract GetValue: unit -> string

type MyRecord =
    { MyField1: int
      MyField2: string }

    interface IMyInterface with
        member x.GetValue() = x.MyField2
"""

    [<Fact>]
    let ``Produces a class with a interface member``() =
        Oak() {

            AnonymousModule() {
                TypeDefn("Meh") { AbstractMember("Name", String()) }

                TypeDefn("Person", UnitPat()) {
                    InterfaceWith(LongIdent "Meh") {
                        Member(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }
                }
            }
        }
        |> produces
            """
type Meh =
    abstract Name: string

type Person() =
    interface Meh with
        member this.Name = "23"
"""

    [<Fact>]
    let ``Produces a class implementing interfaces``() =
        Oak() {

            AnonymousModule() {
                TypeDefn("IFoo") { AbstractMember("Name", String()) }

                InterfaceEnd("IFoo2") { () }

                TypeDefn("IFoo3") { AbstractMember("Name", String()) }

                InterfaceEnd("IFoo4") { () }

                TypeDefn("Person", UnitPat()) {
                    InterfaceWith(LongIdent "IFoo") {
                        Member(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }

                    InterfaceWith("IFoo2") { () }

                    InterfaceWith(LongIdent "IFoo3") {
                        Member(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }

                    InterfaceWith("IFoo4") { () }
                }
            }
        }
        |> produces
            """
type IFoo =
    abstract Name: string

type IFoo2 = interface end

type IFoo3 =
    abstract Name: string

type IFoo4 = interface end

type Person() =
    interface IFoo with
        member this.Name = "23"

    interface IFoo2

    interface IFoo3 with
        member this.Name = "23"

    interface IFoo4
"""

    [<Fact>]
    let ``InterfaceWith supports yield! multiple bindings``() =
        Oak() {
            AnonymousModule() {
                // Define an interface with two abstract members
                TypeDefn("IMyInterface") {
                    AbstractMember("GetValue", [ Unit() ], String())
                    AbstractMember("GetOther", [ Unit() ], Int())
                }

                // Implement the interface on a class using yield! inside InterfaceWith { ... }
                TypeDefn("Person", UnitPat()) {
                    InterfaceWith(LongIdent "IMyInterface") {
                        yield!
                            [ Member("x.GetValue", UnitPat(), ConstantExpr(String("x")))
                              Member("x.GetOther", UnitPat(), ConstantExpr(Int(42))) ]
                    }
                }
            }
        }
        |> produces
            """
type IMyInterface =
    abstract GetValue: unit -> string
    abstract GetOther: unit -> int

type Person() =
    interface IMyInterface with
        member x.GetValue() = "x"
        member x.GetOther() = 42
"""
