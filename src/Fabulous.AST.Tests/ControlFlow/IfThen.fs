namespace Fabulous.AST.Tests.ControlFlow

open FSharp.Compiler.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module IfThen =

    [<Test>]
    let ``Produces if-then expression`` () =
        AnonymousModule() {
            IfThen("x", "=", "12") {
                Value("x", "4")
                Unit()
            }
        }
        |> produces
            """

if x = 12 then
    let x = 4
    ()
"""

    [<Test>]
    let ``Produces if-then expression with EscapeHatch`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            IfThen("x", "=", "12") {
                Value("x", "4")
                EscapeHatch(ComputationExpressionStatement.OtherStatement(thenExpr))
            }
        }
        |> produces
            """

if x = 12 then
    let x = 4
    ()
"""
