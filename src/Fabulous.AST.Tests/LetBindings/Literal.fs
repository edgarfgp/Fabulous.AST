namespace Fabulous.AST.Tests.LetBindings

open Fantomas.FCS.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework
open Fabulous.AST.Tests
open Fabulous.AST

open type Ast

module Literal =
    [<Test>]
    let ``Produces a Literal constant`` () =
        AnonymousModule() { Literal("x", "12") }
        |> produces
            """
[<Literal>]
let x = 12

"""

    [<Test>]
    let ``Produces multiple Literal constants`` () =
        let images =
            [ "Daisy", "daisy.png"
              "Rose", "rose.png"
              "Tulip", "tulip.png"
              "Sunflower", "sunflower.png" ]

        AnonymousModule() {
            for name, value in images do
                Literal(name, $"\"{value}\"")
        }
        |> produces
            """
[<Literal>]
let Daisy = "daisy.png"

[<Literal>]
let Rose = "rose.png"

[<Literal>]
let Tulip = "tulip.png"

[<Literal>]
let Sunflower = "sunflower.png"

"""

    [<Test>]
    let ``Produces a Literal constant with xml docs`` () =
        AnonymousModule() { Literal("x", "12").xmlDocs([ "/// This is a comment" ]) }
        |> produces
            """
/// This is a comment
[<Literal>]
let x = 12

"""

    [<Test>]
    let ``Produces Literal constant with an access control `` () =
        AnonymousModule() { Literal("x", "12").accessibility(AccessControl.Internal) }
        |> produces
            """

[<Literal>]
let internal x = 12

"""

    [<Test>]
    let ``Produces Literal constant with escape hatch`` () =
        AnonymousModule() {
            BindingNode(
                None,
                Some(
                    MultipleAttributeListNode(
                        [ AttributeListNode(
                              SingleTextNode("[<", Range.Zero),
                              [ AttributeNode(
                                    IdentListNode(
                                        [ IdentifierOrDot.Ident(SingleTextNode("Literal", Range.Zero)) ],
                                        Range.Zero
                                    ),
                                    None,
                                    None,
                                    Range.Zero
                                ) ],
                              SingleTextNode(">]", Range.Zero),
                              Range.Zero
                          ) ],
                        Range.Zero
                    )
                ),
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
        
[<Literal>]
let x = 12

"""
