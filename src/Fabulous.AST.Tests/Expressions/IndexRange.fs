namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module IndexRange =

    [<Fact>]
    let ``let value with a IndexRange expression``() =
        Oak() {
            AnonymousModule() {
                IndexRangeExpr()
                IndexFromRangeExpr(ConstantExpr(Int(0)))
                IndexToRangeExpr(ConstantExpr(Int(10)))
                IndexRangeExpr(ConstantExpr(Int(0)), ConstantExpr(Int(10)))

            }
        }
        |> produces
            """
..
0..
..10
0..10
"""
