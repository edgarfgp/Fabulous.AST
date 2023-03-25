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
        NestedModule("A") { Let("x", "12") }
        |> produces
            """

module A =
    let x = 12

"""

    [<Test>]
    let ``Produces a NestedModule using escape hatch`` () =
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
        |> produces
            """

module A =
    let x = 12

"""

    [<Test>]
    let ``Produces a recursive NestedModule`` () =
        (NestedModule("A") { Let("x", "12") }).isRecursive()
        |> produces
            """

module rec A =
    let x = 12

"""

    [<Test>]
    let ``Produces a private NestedModule`` () =
        (NestedModule("A") { Let("x", "12") }).accessibility(AccessControl.Private)
        |> produces
            """

module private A =
    let x = 12

"""

    [<Test>]
    let ``Produces a internal NestedModule`` () =
        (NestedModule("A") { Let("x", "12") }).accessibility(AccessControl.Internal)
        |> produces
            """

module internal A =
    let x = 12
    
"""
