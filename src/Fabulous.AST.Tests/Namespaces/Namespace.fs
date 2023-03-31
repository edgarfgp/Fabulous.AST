namespace Fabulous.AST.Tests.Namespaces

open Fabulous.AST.Tests
open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module Namespace =
    [<Test>]
    let ``Produces a namespace with binding`` () =
        Namespace("Fabulous.AST") { Let("x", "3") }
        |> produces
            """
namespace Fabulous.AST

let x = 3
"""

    [<Test>]
    let ``Produces a namespace  with unit`` () =
        Namespace("Fabulous.AST") { Unit() }
        |> produces
            """
namespace Fabulous.AST

()
"""

    [<Test>]
    let ``Produces a namespace with IdentListNode`` () =

        Namespace(
            IdentListNode(
                [ IdentifierOrDot.Ident(SingleTextNode("Fabulous", Range.Zero))
                  IdentifierOrDot.KnownDot(SingleTextNode(".", Range.Zero))
                  IdentifierOrDot.Ident(SingleTextNode("AST", Range.Zero)) ],
                Range.Zero
            )
        ) {
            Let("x", "3")
        }
        |> produces
            """
namespace Fabulous.AST

let x = 3
"""

    [<Test>]
    let ``Produces a namespace with IdentListNode and BindingNode`` () =
        Namespace(
            IdentListNode(
                [ IdentifierOrDot.Ident(SingleTextNode("Fabulous", Range.Zero))
                  IdentifierOrDot.KnownDot(SingleTextNode(".", Range.Zero))
                  IdentifierOrDot.Ident(SingleTextNode("AST", Range.Zero)) ],
                Range.Zero
            )
        ) {
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
namespace Fabulous.AST

let x = 12
"""

    [<Test>]
    let ``Produces a namespace with nested module`` () =
        Namespace("Fabulous") {
            NestedModule("Fabulous.AST") {
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
namespace Fabulous

module Fabulous.AST =
    let x = 12
"""
