namespace Fabulous.AST.Tests

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Fabulous.AST.Ast

module StringVariant =

    [<Fact>]
    let ``StringVariant produces Quoted values``() =
        Oak() { AnonymousModule() { Value("x", DoubleQuoted "12") } }
        |> produces
            """

let x = "12"

"""
