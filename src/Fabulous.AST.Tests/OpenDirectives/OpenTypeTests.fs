namespace Fabulous.AST.Tests

open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module OpenTypeTests =

    [<Test>]
    let ``Produces a simple open type directive from a string`` () =
        AnonymousModule() { OpenType("ABC") }
        |> produces
            """

open type ABC

"""

    [<Test>]
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
