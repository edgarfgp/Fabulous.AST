namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AppSingleParenArg =

    [<Fact>]
    let ``let value with a AppSingleParenArg expression``() =
        Oak() { AnonymousModule() { AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ "0"; "0"; "0" ]))) } }
        |> produces
            """
MyClass (0, 0, 0)
"""
