namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Microsoft.FSharp.Core
open Xunit

open type Ast

module InterfaceMembers =
    [<Fact>]
    let ``Produces a record with TypeParams and interface member``() =
        AnonymousModule() {
            Interface("IMyInterface") { AbstractCurriedMethod("GetValue", [ Unit() ], String()) }

            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", LongIdent("string"))
                Field("Blue", LongIdent("'other"))
                Field("Yellow", LongIdent("int"))
            })
                .members() {
                InterfaceMember("IMyInterface") {
                    Method("x.GetValue", UnitPat(), ConstantExpr("x.MyField2").hasQuotes(false))
                }
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

    [<Fact>]
    let ``Produces a record with interface member``() =

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
                InterfaceMember("IMyInterface") {
                    Method("x.GetValue", UnitPat(), ConstantExpr("x.MyField2").hasQuotes(false))
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
        AnonymousModule() {
            Interface("Meh") { AbstractProperty("Name", String()) }

            Class("Person") { InterfaceMember("Meh") { Property("this.Name", ConstantExpr("23")) } }
        }
        |> produces
            """
type Meh =
    abstract member Name: string

type Person () =
    interface Meh with
        member this.Name = "23"
"""
