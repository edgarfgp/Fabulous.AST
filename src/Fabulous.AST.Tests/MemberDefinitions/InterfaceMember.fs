namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open Microsoft.FSharp.Core
open NUnit.Framework

module InterfaceMembers =
    [<Test>]
    let ``Produces a record with TypeParams and interface member`` () =
        AnonymousModule() {
            Interface("IMyInterface") { AbstractCurriedMethodMember("GetValue", [ Unit() ], String()) }

            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", TypeLongIdent("string"))
                Field("Blue", TypeLongIdent("'other"))
                Field("Yellow", TypeLongIdent("int"))
            })
                .members() {
                InterfaceMember("IMyInterface") { Method("x.GetValue", UnitPat(), ConstantExpr("x.MyField2")) }
            }
        }

        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    interface IMyInterface with
        member x.GetValue() = x.MyField2

"""

    [<Test>]
    let ``Produces a record with interface member`` () =

        AnonymousModule() {
            Interface("IMyInterface") {
                let parameters = [ Unit() ]
                AbstractCurriedMethodMember("GetValue", parameters, String())
            }

            (Record("MyRecord") {
                Field("MyField1", TypeLongIdent("int"))
                Field("MyField2", TypeLongIdent("string"))
            })
                .members() {
                InterfaceMember("IMyInterface") { Method("x.GetValue", UnitPat(), ConstantExpr("x.MyField2")) }
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

    [<Test>]
    let ``Produces a class with a interface member`` () =
        AnonymousModule() {
            Interface("Meh") { AbstractPropertyMember("Name", String()) }

            Class("Person") { InterfaceMember("Meh") { Property("this.Name", ConstantStringExpr("23")) } }
        }
        |> produces
            """
type Meh =
    abstract member Name: string

type Person () =
    interface Meh with
        member this.Name = "23"
"""
