namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Or =

    [<Fact>]
    let ``let value with a Or pattern`` () =
        AnonymousModule() { Value(OrPat(NamedPat("A"), NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A | B = 12
"""

    [<Fact>]
    let ``let value with a Or custom middle pattern`` () =
        AnonymousModule() { Value(OrPat(NamedPat("A"), "^", NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A ^ B = 12
"""
