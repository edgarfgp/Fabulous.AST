namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ListCons =

    [<Test>]
    let ``let value with a ListCons pattern`` () =
        AnonymousModule() { Value(ListConsPat(NamedPat("a"), NamedPat("b")), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a :: b = 12
"""

    [<Test>]
    let ``let value with a custom ListCons pattern`` () =
        AnonymousModule() {
            Value(ListConsPat(NamedPat("a"), ";;", NamedPat("b")), ConstantExpr(Constant("12", false)))
        }
        |> produces
            """
let a ;; b = 12
"""
