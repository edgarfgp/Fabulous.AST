namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module OpenType =

    [<Fact>]
    let ``Produces a simple open type directive from a string`` () =
        AnonymousModule() { OpenType("ABC") }
        |> produces
            """

open type ABC

"""

    [<Fact>]
    let ``Produces an open type directive from a node`` () =
        AnonymousModule() {
            OpenType(
                IdentListNode(
                    [ IdentifierOrDot.Ident(SingleTextNode("Fabulous", Range.Zero))
                      IdentifierOrDot.KnownDot(SingleTextNode(".", Range.Zero))
                      IdentifierOrDot.Ident(SingleTextNode("Avalonia", Range.Zero))
                      IdentifierOrDot.KnownDot(SingleTextNode(".", Range.Zero))
                      IdentifierOrDot.Ident(SingleTextNode("View", Range.Zero)) ],
                    Range.Zero
                )
            )
        }
        |> produces
            """

open type Fabulous.Avalonia.View

"""
