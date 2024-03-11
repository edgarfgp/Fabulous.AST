namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module App =

    [<Fact>]
    let ``let value with a App expression``() =
        AnonymousModule() { Value("x", AppExpr("printfn") { ConstantExpr("a") }) }
        |> produces
            """

let x = printfn "a"
"""
