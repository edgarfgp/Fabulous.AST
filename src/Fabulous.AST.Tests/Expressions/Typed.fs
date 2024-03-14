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
                Value("x", TypedExpr(ConstantExpr(Constant("2").hasQuotes(false)), ":", LongIdent("string")))
            }
        }
        |> produces
            """

let x = 2: string
"""
