namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module RecordExpr =

    [<Fact>]
    let ``RecordExpr expression`` () =
        AnonymousModule() { RecordExpr() { RecordFieldExpr("A", ConstantExpr("1", false)) } }
        |> produces
            """
{ A = 1 }
"""

    [<Fact>]
    let ``RecordExpr expression with copy info`` () =
        AnonymousModule() { RecordExpr(ConstantExpr("A", false)) { RecordFieldExpr("B", ConstantExpr("1", false)) } }
        |> produces
            """
{ A with B = 1 }
"""

    [<Fact>]
    let ``AnonRecordExpr expression`` () =
        AnonymousModule() { AnonRecordExpr() { RecordFieldExpr("A", ConstantExpr("1", false)) } }
        |> produces
            """
{| A = 1 |}
"""

    [<Fact>]
    let ``AnonRecordExpr expression with copy info`` () =
        AnonymousModule() {
            AnonRecordExpr(ConstantExpr("A", false)) { RecordFieldExpr("B", ConstantExpr("1", false)) }
        }
        |> produces
            """
{| A with B = 1 |}
"""
