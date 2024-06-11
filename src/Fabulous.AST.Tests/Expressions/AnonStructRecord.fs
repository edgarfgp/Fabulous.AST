namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AnonStructRecord =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "`` Red Blue ``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "`` net6.0 ``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces an AnonStructRecordExpr with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { AnonStructRecordExpr([ RecordFieldExpr(value, ConstantExpr(Int 1)) ]) } }
        |> produces
            $$"""

struct {| {{expected}} = 1 |}

"""

    [<Fact>]
    let ``AnonStructRecordExpr expression``() =
        Oak() { AnonymousModule() { AnonStructRecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]) } }
        |> produces
            """
struct {| A = 1 |}
"""

    [<Fact>]
    let ``AnonStructRecordExpr expression with copy info``() =
        Oak() {
            AnonymousModule() {
                AnonStructRecordExpr(ConstantExpr(Constant "A"), [ RecordFieldExpr("B", ConstantExpr(Int 1)) ])
            }
        }
        |> produces
            """
struct {| A with B = 1 |}
"""
