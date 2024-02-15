namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module App =

    [<Test>]
    let ``let value with a App expression`` () =
        AnonymousModule() { Value("x", AppExpr("printfn") { ConstantExpr(ConstantString "\"a\"") }) }
        |> produces
            """

let x = printfn "a"
"""
