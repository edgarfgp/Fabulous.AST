namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module BasicPatterns =

    [<Fact>]
    let ``Constant pattern``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Int(42)), ConstantExpr(String("Forty-two"))) } }
        |> produces
            """
let 42 = "Forty-two"
"""

    [<Fact>]
    let ``Constant pattern from string``() =
        Oak() { AnonymousModule() { Value(ConstantPat("42"), ConstantExpr(String("Forty-two"))) } }
        |> produces
            """
let 42 = "Forty-two"
"""

    [<Fact>]
    let ``Null pattern``() =
        Oak() { AnonymousModule() { Value(NullPat(), ConstantExpr(String("Value for null"))) } }
        |> produces
            """
let null = "Value for null"
"""

    [<Fact>]
    let ``Wildcard pattern``() =
        Oak() { AnonymousModule() { Value(WildPat(), ConstantExpr(String("Wildcard value"))) } }
        |> produces
            """
let _ = "Wildcard value"
"""

    [<Fact>]
    let ``Unit pattern``() =
        Oak() { AnonymousModule() { Value(UnitPat(), ConstantExpr(String("Unit value"))) } }
        |> produces
            """
let () = "Unit value"
"""
