namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module New =

    [<Test>]
    let ``let value with a New expression`` () =
        AnonymousModule() { Value("x", NewExpr(CommonType.mkLongIdent("MyType"), ConstantExpr("12"))) }
        |> produces
            """

let x = new MyType 12
"""

    [<Test>]
    let ``let value with a New expression with parenthesis`` () =
        AnonymousModule() { Value("x", NewExpr(CommonType.mkLongIdent("MyType"), ParenExpr(ConstantExpr("12")))) }
        |> produces
            """

let x = new MyType(12)
"""
