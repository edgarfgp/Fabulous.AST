namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Dynamic =
    [<Fact>]
    let ``Dynamic expression``() =
        Oak() {
            AnonymousModule() {
                DynamicExpr(Int(12), Int(12))

                DynamicExpr(ConstantExpr(Int(12)), AppExpr("failwith", String("Not implemented")))

                DynamicExpr(Int(12), FailWithExpr(String("Not implemented")))
            }
        }
        |> produces
            """
12?12
12?failwith "Not implemented"
12?failwith "Not implemented"
"""
