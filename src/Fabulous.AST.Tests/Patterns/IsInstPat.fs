namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module IsInstPat =

    [<Test>]
    let ``let value with a IsInstPat pattern`` () =
        AnonymousModule() {
            MatchExpr(ConstantExpr(Constant("12", false))) {
                MatchClauseExpr(IsInstPat(String()), ConstantExpr(Constant("12", false)))
            }
        }
        |> produces
            """
match 12 with
| :? string -> 12
"""


    [<Test>]
    let ``let value with a custom IsInstPat pattern`` () =
        AnonymousModule() {
            MatchExpr(ConstantExpr(Constant("12", false))) {
                MatchClauseExpr(IsInstPat("<:", "string"), ConstantExpr(Constant("12", false)))
            }
        }
        |> produces
            """

match 12 with
| <: string -> 12
"""
