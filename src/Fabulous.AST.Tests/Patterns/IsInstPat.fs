namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module IsInstPat =

    [<Fact>]
    let ``let value with a IsInstPat pattern``() =
        AnonymousModule() {
            MatchExpr(ConstantExpr(Constant("12").hasQuotes(false))) {
                MatchClauseExpr(IsInstPat(String()), ConstantExpr(Constant("12").hasQuotes(false)))
            }
        }
        |> produces
            """
match 12 with
| :? string -> 12
"""

    [<Fact>]
    let ``let value with a custom IsInstPat pattern``() =
        AnonymousModule() {
            MatchExpr(ConstantExpr(Constant("12").hasQuotes(false))) {
                MatchClauseExpr(IsInstPat("<:", "string"), ConstantExpr(Constant("12").hasQuotes(false)))
            }
        }
        |> produces
            """

match 12 with
| <: string -> 12
"""
