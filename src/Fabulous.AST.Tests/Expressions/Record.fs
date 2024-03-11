namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module RecordExpr =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "``Red Blue``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "``net6.0``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces an AnonRecordExpr with fields with backticks`` (value: string) (expected: string) =
        AnonymousModule() { AnonRecordExpr() { RecordFieldExpr(value, ConstantExpr("1", false)) } }
        |> produces
            $$"""

{| {{expected}} = 1 |}

"""

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "``Red Blue``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "``net6.0``")>]
    let ``Produces an RecordExpr with fields with backticks`` (value: string) (expected: string) =
        AnonymousModule() { RecordExpr() { RecordFieldExpr(value, ConstantExpr("1", false)) } }
        |> produces
            $$"""
    
{ {{expected}} = 1 }
    
    """

    [<Fact>]
    let ``RecordExpr expression``() =
        AnonymousModule() { RecordExpr() { RecordFieldExpr("A", ConstantExpr("1", false)) } }
        |> produces
            """
{ A = 1 }
"""

    [<Fact>]
    let ``RecordExpr expression with copy info``() =
        AnonymousModule() { RecordExpr(ConstantExpr("A", false)) { RecordFieldExpr("B", ConstantExpr("1", false)) } }
        |> produces
            """
{ A with B = 1 }
"""

    [<Fact>]
    let ``AnonRecordExpr expression``() =
        AnonymousModule() { AnonRecordExpr() { RecordFieldExpr("A", ConstantExpr("1", false)) } }
        |> produces
            """
{| A = 1 |}
"""

    [<Fact>]
    let ``AnonRecordExpr expression with copy info``() =
        AnonymousModule() {
            AnonRecordExpr(ConstantExpr("A", false)) { RecordFieldExpr("B", ConstantExpr("1", false)) }
        }
        |> produces
            """
{| A with B = 1 |}
"""
