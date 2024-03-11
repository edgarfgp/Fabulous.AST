namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module As =

    [<Fact>]
    let ``let value with a As pattern``() =
        AnonymousModule() { Value(AsPat(NamedPat("A"), NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A as B = 12
"""

    [<Fact>]
    let ``let value with a As custom middle pattern``() =
        AnonymousModule() { Value(AsPat(NamedPat("A"), "^", NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A ^ B = 12
"""
