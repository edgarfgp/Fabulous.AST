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
                    Property(ConstantPat(Constant("Message")), ConstantExpr(String(""))).toStatic()
                }
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
                              Property(ConstantPat(Constant("Message")), ConstantExpr(String(""))).toStatic()
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
