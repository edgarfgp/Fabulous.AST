namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module New =

    [<Fact>]
    let ``let value with a New expression``() =
        Oak() { AnonymousModule() { Value("x", NewExpr(LongIdent("MyType"), ConstantExpr(Constant(Unquoted "12")))) } }
        |> produces
            """

let x = new MyType 12
"""

    [<Fact>]
    let ``let value with a New expression with parenthesis``() =
        Oak() { AnonymousModule() { Value("x", NewExpr("MyType", ParenExpr(ConstantExpr(Constant(Unquoted "12"))))) } }
        |> produces
            """

let x = new MyType(12)
"""
