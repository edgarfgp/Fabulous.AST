namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lazy =

    [<Fact>]
    let ``let value with a lazy expression``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Constant("lazy 12"))) } }
        |> produces
            """

let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression widgets``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), LazyExpr(ConstantExpr(Int(12))))
                Value(ConstantPat(Constant("x")), LazyExpr(Int(12)))
                Value(ConstantPat(Constant("x")), LazyExpr("12"))
            }
        }
        |> produces
            """
let x = lazy 12
let x = lazy 12
let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression in parenthesis``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), LazyExpr(ParenExpr(ConstantExpr(Int(12))))) } }
        |> produces
            """

let x = lazy (12)
"""
