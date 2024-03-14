namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module Module =
    [<Fact>]
    let ``Produces a module with binding``() =
        Oak() { ModuleOrNamespace("Fabulous.AST") { Value("x", "3").hasQuotes(false) } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a recursive module``() =
        Oak() {
            ModuleOrNamespace("Fabulous.AST") { Value("x", "3").hasQuotes(false) }
            |> _.toRecursive()
        }
        |> produces
            """
module rec Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a module with unit``() =
        Oak() { ModuleOrNamespace("Fabulous.AST") { ConstantExpr(ConstantUnit()) } }
        |> produces
            """
module Fabulous.AST

()
"""

    [<Fact>]
    let ``Produces a module with IdentListNode``() =
        Oak() { ModuleOrNamespace("Fabulous.AST") { Value("x", "3").hasQuotes(false) } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a module with IdentListNode and BindingNode``() =
        Oak() {
            ModuleOrNamespace("Fabulous.AST") {
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                    Range.Zero
                )
            }
        }
        |> produces
            """
module Fabulous.AST

let x = 12
"""
