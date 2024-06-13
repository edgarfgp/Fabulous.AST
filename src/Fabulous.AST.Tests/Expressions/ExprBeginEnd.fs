namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ExprBeginEnd =

    [<Fact>]
    let ``let value with a ExplicitConstructorThen expression``() =
        Oak() { AnonymousModule() { BeginEndExpr(ConstantExpr(Constant("x"))) } }
        |> produces
            """

begin x end
"""

    [<Fact>]
    let ``let value with a ExplicitConstructorThen constant widget``() =
        Oak() {
            AnonymousModule() {
                BeginEndExpr(Constant("x"))
                BeginEndExpr(Int(10))
            }
        }
        |> produces
            """
begin x end
begin 10 end
"""

    [<Fact>]
    let ``let value with a ExprBeginEnd string``() =
        Oak() { AnonymousModule() { BeginEndExpr("x") } }
        |> produces
            """
begin x end
"""

    [<Fact>]
    let ``let value with a ExprBeginEnd expression in parenthesis``() =
        Oak() { AnonymousModule() { BeginEndExpr(ParenExpr(ConstantExpr(Constant("x")))) } }
        |> produces
            """
begin (x) end
"""
