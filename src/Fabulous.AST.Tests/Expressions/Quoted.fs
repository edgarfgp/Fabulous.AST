namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Quoted =
    [<Fact>]
    let ``let value with a Quoted expression``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), QuotedExpr(ConstantExpr(Int(12))))
                Value(ConstantPat(Constant("x")), QuotedExpr(Int(12)))
                Value(ConstantPat(Constant("x")), QuotedExpr "12")
            }
        }
        |> producesValid
            """

let x = <@ 12 @>
let x = <@ 12 @>
let x = <@ 12 @>
"""

    [<Fact>]
    let ``Untyped quotation``() =
        Oak() {
            AnonymousModule() {
                Value("a", UntypedQuotedExpr(InfixAppExpr(Int(1), "+", Int(1))))
                Value("b", UntypedQuotedExpr(Int(12)))
                Value("c", UntypedQuotedExpr("x"))
            }
        }
        |> producesValid
            """
let a = <@@ 1 + 1 @@>
let b = <@@ 12 @@>
let c = <@@ x @@>
"""
