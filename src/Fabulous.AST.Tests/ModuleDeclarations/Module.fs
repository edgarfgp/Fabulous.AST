namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit

open Fabulous.AST

open type Ast

module Module =
    [<Fact>]
    let ``Produces a top level module``() =
        Oak() { ImplicitNamespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with xml docs``() =
        Oak() {
            ImplicitNamespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.xmlDocs([ "Im a TopLevelModule" ])
        }
        |> produces
            """
/// Im a TopLevelModule
module Fabulous.AST

let x = 3
    """

    [<Fact>]
    let ``Produces a top level module with an attribute``() =
        Oak() {
            ImplicitNamespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.attribute(Attribute("AutoOpen"))
        }
        |> produces
            """
[<AutoOpen>]
module Fabulous.AST

let x = 3
    """

    [<Fact>]
    let ``Produces a top level module with an attribute and xmldocs``() =
        Oak() {
            ImplicitNamespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.attribute(Attribute("AutoOpen"))
            |> _.xmlDocs([ "Im a TopLevelModule" ])
        }
        |> produces
            """
/// Im a TopLevelModule
[<AutoOpen>]
module Fabulous.AST

let x = 3
    """

    [<Fact>]
    let ``Produces a recursive top level module``() =
        Oak() {
            ImplicitNamespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.toRecursive()
        }
        |> produces
            """
module rec Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with unit``() =
        Oak() { ImplicitNamespace("Fabulous.AST") { ConstantExpr(ConstantUnit()) } }
        |> produces
            """
module Fabulous.AST

()
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode``() =
        Oak() { ImplicitNamespace("Fabulous.AST") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(3))) } }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode and BindingNode``() =
        Oak() {
            ImplicitNamespace("Fabulous.AST") {
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
