namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module StructTuplePat =

    [<Fact>]
    let ``let value with a StructTuple pattern``() =
        Oak() {
            AnonymousModule() {
                Value(StructTuplePat([ NamedPat("a"); NamedPat("b") ]), ConstantExpr(Int(12)))
                Value(StructTuplePat([ Constant("a"); Constant("b") ]), ConstantExpr(Int(12)))
                Value(StructTuplePat([ "a"; "b" ]), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """

let struct (a, b) = 12
let struct (a, b) = 12
let struct (a, b) = 12
"""
