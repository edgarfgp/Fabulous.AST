namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module BasicExprTests =

    [<Fact>]
    let ``Constant expression``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Int(42))) } }
        |> producesValid
            """
let x = 42
"""

    [<Fact>]
    let ``Constant expression from string``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr("42")) } }
        |> producesValid
            """
let x = 42
"""

    [<Fact>]
    let ``Null expression``() =
        Oak() { AnonymousModule() { Value("x", NullExpr()) } }
        |> producesValid
            """
let x = null
"""

    [<Fact>]
    let ``Unit expression``() =
        Oak() { AnonymousModule() { Value("x", UnitExpr()) } }
        |> producesValid
            """
let x = ()
"""

    [<Fact>]
    let ``Expression yielded directly to module``() =
        Oak() { AnonymousModule() { ConstantExpr(String("Standalone expression")) } }
        |> producesValid
            """
"Standalone expression"
"""

    [<Fact>]
    let ``Expression yielded to module declaration collection``() =
        Oak() { Namespace("MyNamespace") { Module("MyModule") { ConstantExpr(String("Module expression")) } } }
        |> producesValid
            """
namespace MyNamespace

module MyModule =
    "Module expression"
"""
