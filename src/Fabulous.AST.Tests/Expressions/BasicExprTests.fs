namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module BasicExprTests =

    [<Fact>]
    let ``Constant expression``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Int(42))) } }
        |> produces
            """
let x = 42
"""

    [<Fact>]
    let ``Constant expression from string``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr("42")) } }
        |> produces
            """
let x = 42
"""

    [<Fact>]
    let ``Null expression``() =
        Oak() { AnonymousModule() { Value("x", NullExpr()) } }
        |> produces
            """
let x = null
"""

    [<Fact>]
    let ``Unit expression``() =
        Oak() { AnonymousModule() { Value("x", UnitExpr()) } }
        |> produces
            """
let x = ()
"""

    [<Fact>]
    let ``Expression yielded directly to module``() =
        Oak() { AnonymousModule() { ConstantExpr(String("Standalone expression")) } }
        |> produces
            """
"Standalone expression"
"""

    [<Fact>]
    let ``Expression yielded to module declaration collection``() =
        Oak() { Namespace("MyNamespace") { Module("MyModule") { ConstantExpr(String("Module expression")) } } }
        |> produces
            """
namespace MyNamespace

module MyModule =
    "Module expression"
"""
