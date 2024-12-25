namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Microsoft.FSharp.Core
open Xunit

open type Ast

module MemberDefn =
    [<Fact>]
    let ``yield! multiple MemberDefns``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMyInterface") { AbstractMember("GetValue", [ Unit() ], String()) }
                TypeDefn("IMyInterface1") { AbstractMember("GetValue", [ Unit() ], String()) }
                TypeDefn("IMyInterface2") { AbstractMember("GetValue", [ Unit() ], String()) }

                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    yield!
                        [ AnyMemberDefn(
                              InterfaceWith(LongIdent "IMyInterface") {
                                  Member("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                              }
                          )

                          AnyMemberDefn(
                              InterfaceWith(LongIdent "IMyInterface1") {
                                  Member("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                              }
                          ) ]

                    InterfaceWith("IMyInterface2") { () }

                }

            }
        }

        |> produces
            """
type IMyInterface =
    abstract GetValue: unit -> string

type IMyInterface1 =
    abstract GetValue: unit -> string

type IMyInterface2 =
    abstract GetValue: unit -> string

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    interface IMyInterface with
        member x.GetValue() = x.MyField2

    interface IMyInterface1 with
        member x.GetValue() = x.MyField2

    interface IMyInterface2

"""
