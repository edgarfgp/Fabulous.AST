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
        Oak() { Namespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) } |> _.toImplicit() }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with xml docs``() =
        Oak() {
            Namespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.toImplicit()
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
            Namespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.attribute(Attribute("AutoOpen"))
            |> _.toImplicit()
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
            Namespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.attribute(Attribute("AutoOpen"))
            |> _.xmlDocs([ "Im a TopLevelModule" ])
            |> _.toImplicit()
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
            Namespace("Fabulous.AST") { Value("x", ConstantExpr(Int(3))) }
            |> _.toRecursive()
            |> _.toImplicit()
        }
        |> produces
            """
module rec Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with unit``() =
        Oak() { Namespace("Fabulous.AST") { ConstantExpr(ConstantUnit()) } |> _.toImplicit() }
        |> produces
            """
module Fabulous.AST

()
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode``() =
        Oak() {
            Namespace("Fabulous.AST") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(3))) }
            |> _.toImplicit()
        }
        |> produces
            """
module Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a top level module with IdentListNode and BindingNode``() =
        Oak() {
            Namespace("Fabulous.AST") {
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
            |> _.toImplicit()
        }
        |> produces
            """
module Fabulous.AST

let x = 12
"""
