namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module IsInstPat =

    [<Fact>]
    let ``let value with a IsInstPat pattern``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(
                    ConstantExpr(Constant(Unquoted "12")),
                    [ MatchClauseExpr(IsInstPat(String()), ConstantExpr(Constant(Unquoted "12"))) ]
                )
            }
        }
        |> produces
            """
match 12 with
| :? string -> 12
"""

    [<Fact>]
    let ``let value with a custom IsInstPat pattern``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(
                    ConstantExpr(Constant(Unquoted "12")),
                    [ MatchClauseExpr(IsInstPat("<:", "string"), ConstantExpr(Constant(Unquoted "12"))) ]
                )
            }
        }
        |> produces
            """

match 12 with
| <: string -> 12
"""
