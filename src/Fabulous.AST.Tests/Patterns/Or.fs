namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Or =

    [<Test>]
    let ``let value with a Or pattern`` () =
        AnonymousModule() { Value(OrPat(NamedPat("A"), NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A | B = 12
"""

    [<Test>]
    let ``let value with a Or custom middle pattern`` () =
        AnonymousModule() { Value(OrPat(NamedPat("A"), "^", NamedPat("B")), ConstantExpr(Constant("12", false))) }
        |> produces
            """

let A ^ B = 12
"""
