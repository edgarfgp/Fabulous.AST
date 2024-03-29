namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ListCons =

    [<Fact>]
    let ``let value with a ListCons pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ListConsPat(NamedPat("a"), NamedPat("b")), ConstantExpr(Constant(Unquoted "12")))
            }
        }
        |> produces
            """
let a :: b = 12
"""

    [<Fact>]
    let ``let value with a custom ListCons pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ListConsPat(NamedPat("a"), ";;", NamedPat("b")), ConstantExpr(Constant(Unquoted "12")))
            }
        }
        |> produces
            """
let a ;; b = 12
"""
