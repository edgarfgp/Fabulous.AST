namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module LibraryOnlyStaticOptimization =

    [<Fact>]
    let ``LibraryOnlyStaticOptimization expression``() =
        Oak() {
            AnonymousModule() {
                LibraryOnlyStaticOptimizationExpr("a", ConstantExpr(Constant("b")))
                LibraryOnlyStaticOptimizationExpr("a", Constant("b"))
                LibraryOnlyStaticOptimizationExpr("a", "b")

                LibraryOnlyStaticOptimizationExpr(ConstantExpr "a", ConstantExpr "b")
                LibraryOnlyStaticOptimizationExpr(Int(2), "b")
                LibraryOnlyStaticOptimizationExpr(Int(1), Int(2))
                LibraryOnlyStaticOptimizationExpr(Int(1), IdentExpr("b"))

                LibraryOnlyStaticOptimizationExpr(ConstantExpr "a", [ WhenTyparIsStruct("foo") ], ConstantExpr "b")

                LibraryOnlyStaticOptimizationExpr(
                    ConstantExpr "a",
                    [ WhenTyparIsStruct("foo"); WhenTyparTyconEqualsTycon("a", Int()) ],
                    ConstantExpr "b"
                )
            }
        }
        |> produces
            """
a = b
a = b
a = b
a = b
2 = b
1 = 2
1 = b
a when foo = b
a when foo a: int = b
"""
