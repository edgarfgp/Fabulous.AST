namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Typed =
    [<Fact>]
    let ``let value with a typed expression``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), TypedExpr(ConstantExpr(Int(2)), ":", LongIdent("string")))
                Value(ConstantPat(Constant("x")), TypedExpr(Int(2), ":", LongIdent("string")))
                Value(ConstantPat(Constant("x")), TypedExpr("2", ":", "string"))
            }
        }
        |> produces
            """

let x = 2: string
let x = 2: string
let x = 2: string
"""
