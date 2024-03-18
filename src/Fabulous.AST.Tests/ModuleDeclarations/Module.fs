namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module Module =
    [<Fact>]
    let ``Produces a top level module``() =
        Oak() { TopLevelModule("Fabulous.AST") { Value("x", Unquoted "3") } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a recursive top level module``() =
        Oak() { TopLevelModule("Fabulous.AST") { Value("x", Unquoted "3") } |> _.toRecursive() }
        |> produces
            """
module rec Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with unit``() =
        Oak() { TopLevelModule("Fabulous.AST") { ConstantExpr(ConstantUnit()) } }
        |> produces
            """
module Fabulous.AST

()
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode``() =
        Oak() { TopLevelModule("Fabulous.AST") { Value("x", Unquoted "3") } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode and BindingNode``() =
        Oak() {
            TopLevelModule("Fabulous.AST") {
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
