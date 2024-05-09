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
                IndexFromRangeExpr (Int(0))
                IndexFromRangeExpr "0"
                IndexToRangeExpr(ConstantExpr(Int(10)))
                IndexToRangeExpr (Int(10))
                IndexToRangeExpr "10"
                IndexRangeExpr(ConstantExpr(Int(0)), ConstantExpr(Int(10)))
                IndexRangeExpr(Int(0), Int(10))
                IndexRangeExpr("0", "10")

            }
        }
        |> produces
            """
..
0..
0..
0..
..10
..10
..10
0..10
0..10
0..10
"""
