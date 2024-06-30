namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrListPat =

    [<Fact>]
    let ``let value with a List pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ListPat([ NamedPat("a"); NamedPat("b") ]), ConstantExpr(Int(12)))
                Value(ListPat([ Constant("a"); Constant("b") ]), ConstantExpr(Int(12)))
                Value(ListPat([ "a"; "b" ]), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """

let [ a; b ] = 12
let [ a; b ] = 12
let [ a; b ] = 12
"""

    [<Fact>]
    let ``let value with an Array pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ArrayPat([ NamedPat("a"); NamedPat("b") ]), ConstantExpr(Int(12)))
                Value(ArrayPat([ Constant("a"); Constant("b") ]), ConstantExpr(Int(12)))
                Value(ArrayPat([ "a"; "b" ]), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let [| a; b |] = 12
let [| a; b |] = 12
let [| a; b |] = 12
"""
