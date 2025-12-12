namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit
open Fabulous.AST.Tests
open Fabulous.AST

open type Ast

module Literal =
    [<Fact>]
    let ``Produces a Literal constant``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).attribute(Attribute "Literal")
            }
        }
        |> produces
            """
[<Literal>]
let x = 12

"""

    [<Fact>]
    let ``Produces multiple Literal constants``() =
        let images =
            [ "Daisy", "daisy.png"
              "Rose", "rose.png"
              "Tulip", "tulip.png"
              "Sunflower", "sunflower.png" ]

        Oak() {
            AnonymousModule() {
                for name, value in images do
                    Value(ConstantPat(Constant(name)), ConstantExpr(String(value))).attribute(Attribute "Literal")
            }
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

    [<Fact>]
    let ``Produces a Literal constant with xml docs``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .attribute(Attribute "Literal")
                    .xmlDocs([ "This is a comment" ])
            }
        }
        |> produces
            """
/// This is a comment
[<Literal>]
let x = 12

"""

    [<Fact>]
    let ``Produces Literal constant with an access control ``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).attribute(Attribute "Literal").toInternal()
            }
        }
        |> produces
            """

[<Literal>]
let internal x = 12

"""

    [<Fact>]
    let ``Produces Literal constant with escape hatch``() =
        Oak() {
            AnonymousModule() {
                EscapeHatch(
                    ModuleDecl.TopLevelBinding(
                        BindingNode(
                            None,
                            Some(
                                MultipleAttributeListNode(
                                    [ AttributeListNode(
                                          SingleTextNode("[<", Range.Zero),
                                          [ SyntaxOak.AttributeNode(
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
                            MultipleTextsNode([ SingleTextNode.Create("let") ], Range.Zero),
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
                    )
                )
            }
        }
        |> produces
            """

[<Literal>]
let x = 12

"""
