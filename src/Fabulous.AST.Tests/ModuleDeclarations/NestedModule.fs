namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module NestedModule =

    [<Fact>]
    let ``Produces a NestedModule``() =
        Oak() { AnonymousModule() { NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) } } }

        |> produces
            """

module A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a NestedModule with xml comments``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
                |> _.xmlDocs([ "I'm a xml comment" ])
            }
        }

        |> produces
            """

/// I'm a xml comment
module A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a NestedModule with an attribute``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
                |> _.attribute(Attribute("AutoOpen"))
            }
        }

        |> produces
            """

[<AutoOpen>]
module A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a NestedModule using escape hatch``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") {
                    BindingNode(
                        None,
                        None,
                        MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                        false,
                        None,
                        None,
                        Choice1Of2(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)
                        ),
                        None,
                        List.Empty,
                        None,
                        SingleTextNode("=", Range.Zero),
                        Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                        Range.Zero
                    )
                }
            }
        }

        |> produces
            """

module A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a recursive NestedModule``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
                |> _.toRecursive()
            }
        }

        |> produces
            """

module rec A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a private NestedModule``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
                |> _.toPrivate()
            }
        }
        |> produces
            """

module private A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a internal NestedModule``() =
        Oak() {
            AnonymousModule() {
                NestedModule("A") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
                |> _.toInternal()
            }
        }
        |> produces
            """

module internal A =
    let x = 12

"""

    [<Fact>]
    let ``Produces a module with nested module``() =
        Oak() {
            Namespace("Fabulous.AST") {
                NestedModule("Foo") {
                    BindingNode(
                        None,
                        None,
                        MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                        false,
                        None,
                        None,
                        Choice1Of2(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)
                        ),
                        None,
                        List.Empty,
                        None,
                        SingleTextNode("=", Range.Zero),
                        Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                        Range.Zero
                    )
                }
            }
        }

        |> produces
            """
namespace Fabulous.AST

module Foo =
    let x = 12
"""

    [<Fact>]
    let ``Produces a module with multiple nested module``() =
        Oak() {
            Namespace("Fabulous.AST") {
                NestedModule("Foo") {
                    BindingNode(
                        None,
                        None,
                        MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                        false,
                        None,
                        None,
                        Choice1Of2(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)
                        ),
                        None,
                        List.Empty,
                        None,
                        SingleTextNode("=", Range.Zero),
                        Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                        Range.Zero
                    )
                }

                NestedModule("Bar") { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) }
            }
        }

        |> produces
            """
namespace Fabulous.AST

module Foo =
    let x = 12

module Bar =
    let x = 12

"""
