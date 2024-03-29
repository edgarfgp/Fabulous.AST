namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Parameter =

    [<Fact>]
    let ``let value with a Parameter pattern``() =
        Oak() { AnonymousModule() { Value(ParameterPat(NamedPat("a")), ConstantExpr(Constant(Unquoted "12"))) } }
        |> produces
            """
let a = 12
"""

    [<Fact>]
    let ``let value with a typed Parameter pattern``() =
        Oak() {
            AnonymousModule() { Value(ParameterPat(NamedPat("a"), String()), ConstantExpr(Constant(Unquoted "12"))) }
        }
        |> produces
            """
let a: string = 12
"""

    [<Fact>]
    let ``let value with a Parameter string pattern``() =
        Oak() { AnonymousModule() { Value(ParameterPat("a"), ConstantExpr(Constant(Unquoted "12"))) } }
        |> produces
            """
let a = 12
"""

    [<Fact>]
    let ``let value with a typed Parameter string pattern``() =
        Oak() { AnonymousModule() { Value(ParameterPat("a", "string"), ConstantExpr(Constant(Unquoted "12"))) } }
        |> produces
            """
let a: string = 12
"""
