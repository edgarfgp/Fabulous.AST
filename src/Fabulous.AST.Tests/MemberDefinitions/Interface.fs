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
                Interface("IMyInterface") { AbstractCurriedMethod("GetValue", [ Unit() ], String()) }
                Interface("IMyInterface2") { AbstractCurriedMethod("GetValue", [ Unit() ], String()) }

                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    InterfaceMember(LongIdent "IMyInterface") {
                        Method("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                    }

                    InterfaceMember("IMyInterface2") { () }
                }

            }
        }

        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

type IMyInterface2 =
    abstract member GetValue: unit -> string

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
                Interface("IMyInterface") {
                    let parameters = [ Unit() ]
                    AbstractCurriedMethod("GetValue", parameters, String())
                }

                (Record("MyRecord") {
                    Field("MyField1", LongIdent("int"))
                    Field("MyField2", LongIdent("string"))
                })
                    .members() {
                    InterfaceMember(LongIdent "IMyInterface") {
                        Method("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                    }
                }
            }
        }
        |> produces
            """

type IMyInterface =
    abstract member GetValue: unit -> string

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
                Interface("Meh") { AbstractProperty("Name", String()) }

                Class("Person") {
                    InterfaceMember(LongIdent "Meh") {
                        Property(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }
                }
            }
        }
        |> produces
            """
type Meh =
    abstract member Name: string

type Person() =
    interface Meh with
        member this.Name = "23"
"""

    [<Fact>]
    let ``Produces a class implementing interfaces``() =
        Oak() {

            AnonymousModule() {
                Interface("IFoo") { AbstractProperty("Name", String()) }

                InterfaceEnd("IFoo2")

                Interface("IFoo3") { AbstractProperty("Name", String()) }

                InterfaceEnd("IFoo4")

                Class("Person") {
                    InterfaceMember(LongIdent "IFoo") {
                        Property(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }

                    InterfaceMember("IFoo2") { () }

                    InterfaceMember(LongIdent "IFoo3") {
                        Property(ConstantPat(Constant("this.Name")), ConstantExpr(String("23")))
                    }

                    InterfaceMember("IFoo4") { () }
                }
            }
        }
        |> produces
            """
type IFoo =
    abstract member Name: string

type IFoo2 = interface end

type IFoo3 =
    abstract member Name: string

type IFoo4 = interface end

type Person() =
    interface IFoo with
        member this.Name = "23"

    interface IFoo2

    interface IFoo3 with
        member this.Name = "23"

    interface IFoo4
"""
