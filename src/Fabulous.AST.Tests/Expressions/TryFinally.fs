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
