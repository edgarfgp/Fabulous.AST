namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TuplePat =

    [<Fact>]
    let ``let value with a Tuple pattern``() =
        Oak() {
            AnonymousModule() {
                Value(TuplePat([ NamedPat("a"); NamedPat("b") ]), ConstantExpr(Int(12)))
                Value(TuplePat([ Constant("a"); Constant("b") ]), ConstantExpr(Int(12)))
                Value(TuplePat([ "a"; "b" ]), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """

let a, b = 12
let a, b = 12
let a, b = 12
"""
