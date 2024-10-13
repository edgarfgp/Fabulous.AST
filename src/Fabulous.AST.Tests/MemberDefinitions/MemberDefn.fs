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
                              InterfaceMember(LongIdent "IMyInterface") {
                                  Method("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                              }
                          )

                          AnyMemberDefn(
                              InterfaceMember(LongIdent "IMyInterface1") {
                                  Method("x.GetValue", UnitPat(), ConstantExpr(Constant "x.MyField2"))
                              }
                          ) ]

                    InterfaceMember("IMyInterface2") { () }

                }

            }
        }

        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

type IMyInterface1 =
    abstract member GetValue: unit -> string

type IMyInterface2 =
    abstract member GetValue: unit -> string

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
