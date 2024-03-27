namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Ands =

    [<Fact>]
    let ``let value with a Ands pattern``() =
        Oak() {
            AnonymousModule() {
                Value(AndsPat([ NamedPat("A"); NamedPat("B") ]), ConstantExpr(Constant(Unquoted "12")))
            }
        }
        |> produces
            """

let A & B = 12
"""
