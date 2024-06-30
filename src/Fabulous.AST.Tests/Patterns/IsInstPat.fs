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
                MatchExpr(ConstantExpr(Int(12)), [ MatchClauseExpr(IsInstPat(String()), ConstantExpr(Int(12))) ])
            }
        }
        |> produces
            """
match 12 with
| :? string -> 12
"""
