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
                Value(RecordPat([ RecordFieldPat("A", ConstantPat(String("3"))) ]), ConstantExpr(Int(12)))

                Value(RecordPat([ RecordFieldPat("B", ConstantPat(String("4")), "x") ]), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let { A = "3" } = 12
let { x.B = "4" } = 12
"""
