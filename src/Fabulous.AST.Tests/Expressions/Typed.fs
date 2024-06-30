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
                Value(ConstantPat(Constant("x")), TypedExpr(ConstantExpr(Int(2)), ":", LongIdent("int")))
                Value(ConstantPat(Constant("x")), TypedExpr(Int(2), ":", LongIdent("int")))
                Value(ConstantPat(Constant("x")), TypedExpr("2", ":", "int"))
            }
        }
        |> produces
            """

let x = 2: int
let x = 2: int
let x = 2: int
"""
