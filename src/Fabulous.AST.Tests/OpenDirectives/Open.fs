namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module Open =

    [<Fact>]
    let ``Produces a simple open directive from a string``() =
        AnonymousModule() { Open("ABC") }
        |> produces
            """

open ABC

"""

    [<Fact>]
    let ``Produces an open directive from a node``() =
        AnonymousModule() {
            Open(
                IdentListNode(
                    [ IdentifierOrDot.Ident(SingleTextNode("ABC", Range.Zero))
                      IdentifierOrDot.KnownDot(SingleTextNode(".", Range.Zero))
                      IdentifierOrDot.Ident(SingleTextNode("DEF", Range.Zero)) ],
                    Range.Zero
                )
            )
        }
        |> produces
            """

open ABC.DEF

"""
