namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module RecordExpr =

    [<Test>]
    let ``RecordExpr expression`` () =
        AnonymousModule() { RecordExpr() { RecordFieldExpr("A", ConstantExpr("1")) } }
        |> produces
            """
{ A = 1 }
"""

    [<Test>]
    let ``RecordExpr expression with copy info`` () =
        AnonymousModule() { RecordExpr(ConstantExpr("A")) { RecordFieldExpr("B", ConstantExpr("1")) } }
        |> produces
            """
{ A with B = 1 }
"""

    [<Test>]
    let ``AnonRecordExpr expression`` () =
        AnonymousModule() { AnonRecordExpr() { RecordFieldExpr("A", ConstantExpr("1")) } }
        |> produces
            """
{| A = 1 |}
"""

    [<Test>]
    let ``AnonRecordExpr expression with copy info`` () =
        AnonymousModule() { AnonRecordExpr(ConstantExpr("A")) { RecordFieldExpr("B", ConstantExpr("1")) } }
        |> produces
            """
{| A with B = 1 |}
"""