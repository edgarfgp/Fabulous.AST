namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ExceptionDefn =

    [<Fact>]
    let ``Produces an ExceptionDefn``() =
        Oak() {
            AnonymousModule() {
                ExceptionDefn("Error")

                ExceptionDefn("Error1", Field("string"))

                ExceptionDefn("Error2", [ Field(String()); Field(Int()) ])

                ExceptionDefn("Error3", Field("msg", String())).members() {
                    Member(ConstantPat(Constant("Message")), ConstantExpr(String(""))).toStatic()
                }

                ExceptionDefn(UnionCase("Error4", [ Field(String()); Field(Int()) ]))

                ExceptionDefn("Error5", [ String(); Int() ])

                ExceptionDefn("Error6", [ "string"; "int" ])

                ExceptionDefn(UnionCase("Error7", [ ("a", String()); ("b", Int()) ]))

                ExceptionDefn("Error8", [ ("a", String()); ("b", Int()) ])

                ExceptionDefn("Error9", [ ("a", "string"); ("b", "int") ])
            }
        }
        |> produces
            """
exception Error
exception Error1 of string
exception Error2 of string * int

exception Error3 of msg: string with
    static member Message = ""

exception Error4 of string * int
exception Error5 of string * int
exception Error6 of string * int
exception Error7 of a: string * b: int
exception Error8 of a: string * b: int
exception Error9 of a: string * b: int
"""

    [<Fact>]
    let ``yield! multiple ExceptionDefn``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ AnyModuleDecl(ExceptionDefn("Error"))

                      AnyModuleDecl(ExceptionDefn("Error1", Field("string")))

                      AnyModuleDecl(ExceptionDefn("Error2", [ Field(String()); Field(Int()) ]))

                      AnyModuleDecl(
                          ExceptionDefn("Error3", Field("msg", String())).members() {
                              Member("Message", String("")).toStatic()
                          }
                      ) ]
            }
        }
        |> produces
            """
exception Error
exception Error1 of string
exception Error2 of string * int

exception Error3 of msg: string with
    static member Message = ""
"""
