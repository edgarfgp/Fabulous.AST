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

                ExceptionDefn("Error3", Field("msg", String()))
            }
        }
        |> produces
            """
exception Error
exception Error1 of string
exception Error2 of string * int
exception Error3 of msg: string
"""
