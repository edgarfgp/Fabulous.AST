namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Paren =

    [<Fact>]
    let ``let value with a Paren pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ParenPat(ConstantPat(Constant("a"))), ConstantExpr(Int(12)))
                Value(ParenPat(Constant("a")), ConstantExpr(Int(12)))
                Value(ParenPat("a"), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let (a) = 12
let (a) = 12
let (a) = 12
"""
