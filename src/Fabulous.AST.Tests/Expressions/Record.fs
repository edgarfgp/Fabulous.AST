namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module RecordExpr =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "`` Red Blue ``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "`` net6.0 ``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces an AnonRecordExpr with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { AnonRecordExpr([ RecordFieldExpr(value, ConstantExpr(Int 1)) ]) } }
        |> produces
            $$"""

{| {{expected}} = 1 |}

"""

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "`` Red Blue ``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "`` net6.0 ``")>]
    let ``Produces an RecordExpr with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { RecordExpr([ RecordFieldExpr(value, ConstantExpr(Int 1)) ]) } }
        |> produces
            $$"""

{ {{expected}} = 1 }

    """

    [<Fact>]
    let ``RecordExpr expression``() =
        Oak() { AnonymousModule() { RecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]) } }
        |> produces
            """
{ A = 1 }
"""

    [<Fact>]
    let ``RecordExpr expression with copy info``() =
        Oak() {
            AnonymousModule() { RecordExpr(ConstantExpr(Constant "A"), [ RecordFieldExpr("B", ConstantExpr(Int 1)) ]) }
        }
        |> produces
            """
{ A with B = 1 }
"""

    [<Fact>]
    let ``AnonRecordExpr expression``() =
        Oak() { AnonymousModule() { AnonRecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]) } }
        |> produces
            """
{| A = 1 |}
"""

    [<Fact>]
    let ``AnonRecordExpr expression with copy info``() =
        Oak() {
            AnonymousModule() {
                AnonRecordExpr(ConstantExpr(Constant "A"), [ RecordFieldExpr("B", ConstantExpr(Int 1)) ])
            }
        }
        |> produces
            """
{| A with B = 1 |}
"""
