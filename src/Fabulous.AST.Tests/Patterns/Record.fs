namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module RecordPat =

    [<Fact>]
    let ``let value with a Record pattern``() =
        Oak() {
            AnonymousModule() {
                Value(RecordPat([ RecordFieldPat(Unquoted("A"), Quoted("3")) ]), ConstantExpr(Constant(Unquoted "12")))

                Value(
                    RecordPat([ RecordFieldPat(Unquoted("B"), Quoted("4"), Unquoted("x")) ]),
                    ConstantExpr(Constant(Unquoted "12"))
                )
            }
        }
        |> produces
            """
let { A = "3" } = 12
let { x.B = "4" } = 12
"""
