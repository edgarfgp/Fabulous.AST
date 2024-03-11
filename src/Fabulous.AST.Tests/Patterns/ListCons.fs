namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ListCons =

    [<Fact>]
    let ``let value with a ListCons pattern``() =
        AnonymousModule() { Value(ListConsPat(NamedPat("a"), NamedPat("b")), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a :: b = 12
"""

    [<Fact>]
    let ``let value with a custom ListCons pattern``() =
        AnonymousModule() {
            Value(ListConsPat(NamedPat("a"), ";;", NamedPat("b")), ConstantExpr(Constant("12", false)))
        }
        |> produces
            """
let a ;; b = 12
"""
