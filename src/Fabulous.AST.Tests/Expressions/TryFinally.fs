namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TryFinally =
    [<Fact>]
    let ``TryFinally expression``() =
        Oak() {
            AnonymousModule() {
                TryFinallyExpr(Int(12), Int(12))

                TryFinallyExpr(ConstantExpr(Int(12)), AppExpr("failwith", String("Not implemented")))

                TryFinallyExpr(Int(12), FailWithExpr(String("Not implemented")))
            }
        }
        |> produces
            """
try
    12
finally
    12

try
    12
finally
    failwith "Not implemented"

try
    12
finally
    failwith "Not implemented"
"""

    [<Fact>]
    let ``TryFinally with Expr value and string finally``() =
        Oak() { AnonymousModule() { TryFinallyExpr(AppExpr("compute", Int(10)), "cleanup()") } }
        |> produces
            """
try
    compute 10
finally
    cleanup()
"""

    [<Fact>]
    let ``TryFinally with string value and Constant finally``() =
        Oak() { AnonymousModule() { TryFinallyExpr("compute()", Int(0)) } }
        |> produces
            """
try
    compute()
finally
    0
"""
