namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Parameter =

    [<Test>]
    let ``let value with a Parameter pattern`` () =
        AnonymousModule() { Value(ParameterPat(NamedPat("a")), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a = 12
"""

    [<Test>]
    let ``let value with a typed Parameter pattern`` () =
        AnonymousModule() { Value(ParameterPat(NamedPat("a"), String()), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a: string = 12
"""

    [<Test>]
    let ``let value with a Parameter string pattern`` () =
        AnonymousModule() { Value(ParameterPat("a"), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a = 12
"""

    [<Test>]
    let ``let value with a typed Parameter string pattern`` () =
        AnonymousModule() { Value(ParameterPat("a", "string"), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let a: string = 12
"""
