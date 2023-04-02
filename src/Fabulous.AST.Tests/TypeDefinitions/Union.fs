namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module Union =

    [<Test>]
    let ``Produces an union`` () =
        AnonymousModule() {
            Union("Colors") {
                UnionCase("Red")
                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            }
        }

        |> produces
            """

type Colors =
    | Red
    | Green
    | Blue
    | Yellow

"""
