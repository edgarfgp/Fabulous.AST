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
        |> producesValid
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
        |> producesValid
            $$"""

{ {{expected}} = 1 }

    """

    [<Fact>]
    let ``RecordExpr expression``() =
        Oak() { AnonymousModule() { RecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]) } }
        |> producesValid
            """
{ A = 1 }
"""

    [<Fact>]
    let ``RecordExpr expression with copy info``() =
        Oak() {
            AnonymousModule() { RecordExpr(ConstantExpr(Constant "A"), [ RecordFieldExpr("B", ConstantExpr(Int 1)) ]) }
        }
        |> producesValid
            """
{ A with B = 1 }
"""

    [<Fact>]
    let ``AnonRecordExpr expression``() =
        Oak() { AnonymousModule() { AnonRecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]) } }
        |> producesValid
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
        |> producesValid
            """
{| A with B = 1 |}
"""

    [<Fact>]
    let ``RecordExpr body mixes spreads and fields in order``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "p2",
                    RecordExpr() {
                        SpreadExpr("p1")
                        RecordFieldExpr("X", Int(1))
                        SpreadExpr(ParenExpr(AppExpr("make", Int(2))))
                    }
                )
            }
        }
        |> producesValid
            """
let p2 = { ...p1; X = 1; ...(make 2) }
"""

    [<Fact>]
    let ``AnonRecordExpr body with a spread``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "p2",
                    AnonRecordExpr() {
                        SpreadExpr("p1")
                        RecordFieldExpr("Z", Int(3))
                    }
                )
            }
        }
        |> producesValid
            """
let p2 = {| ...p1; Z = 3 |}
"""

    [<Fact>]
    let ``RecordExpr body with only fields``() =
        Oak() {
            AnonymousModule() {
                RecordExpr() {
                    RecordFieldExpr("A", Int(1))
                    RecordFieldExpr("B", Int(2))
                }
            }
        }
        |> producesValid
            """
{ A = 1; B = 2 }
"""
