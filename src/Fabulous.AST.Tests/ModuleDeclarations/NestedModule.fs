namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module NestedModule =

    [<Test>]
    let ``Produces a NestedModule`` () =
        AnonymousModule() { NestedModule("A") { Value("x", "12", false) } }

        |> produces
            """

module A =
    let x = 12

"""

    [<Test>]
    let ``Produces a NestedModule using escape hatch`` () =
        AnonymousModule() {
            NestedModule("A") {
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

module A =
    let x = 12

"""

    [<Test>]
    let ``Produces a recursive NestedModule`` () =
        AnonymousModule() { NestedModule("A").toRecursive() { Value("x", "12", false) } }

        |> produces
            """

module rec A =
    let x = 12

"""

    [<Test>]
    let ``Produces a private NestedModule`` () =
        AnonymousModule() { NestedModule("A").accessibility(AccessControl.Private) { Value("x", "12", false) } }
        |> produces
            """

module private A =
    let x = 12

"""

    [<Test>]
    let ``Produces a internal NestedModule`` () =
        AnonymousModule() { NestedModule("A").accessibility(AccessControl.Internal) { Value("x", "12", false) } }
        |> produces
            """

module internal A =
    let x = 12
    
"""
