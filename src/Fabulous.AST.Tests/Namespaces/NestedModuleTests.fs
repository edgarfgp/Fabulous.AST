namespace Fabulous.AST.Tests

open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module NestedModuleTests =

    [<Test>]
    let ``Produces a NestedModule`` () =
        AnonymousModule() { NestedModule("A") { Let("x", "12") } }

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
        AnonymousModule() { NestedModule("A").isRecursive() { Let("x", "12") } }

        |> produces
            """

module rec A =
    let x = 12

"""

    [<Test>]
    let ``Produces a private NestedModule`` () =
        AnonymousModule() { NestedModule("A").accessibility(AccessControl.Private) { Let("x", "12") } }
        |> produces
            """

module private A =
    let x = 12

"""

    [<Test>]
    let ``Produces a internal NestedModule`` () =
        AnonymousModule() { NestedModule("A").accessibility(AccessControl.Internal) { Let("x", "12") } }
        |> produces
            """

module internal A =
    let x = 12
    
"""
