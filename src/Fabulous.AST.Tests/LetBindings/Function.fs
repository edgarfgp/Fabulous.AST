namespace Fabulous.AST.Tests.LetBindings

open FSharp.Compiler.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module Function =

    [<Test>]
    let ``Produces FizzBuzz`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() { Function("x", [| "i" |]) { EscapeHatch(thenExpr) } }
        |> produces
            """

let x i = ()

"""
