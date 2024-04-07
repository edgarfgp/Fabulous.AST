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
                IndexFromRangeExpr("0")
                IndexToRangeExpr("10")
                IndexRangeExpr("0", "10")

            }
        }
        |> produces
            """
..
0..
..10
0..10
"""
