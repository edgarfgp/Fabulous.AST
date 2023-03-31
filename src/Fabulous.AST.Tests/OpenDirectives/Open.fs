namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module Open =

    [<Test>]
    let ``Produces a simple open directive from a string`` () =
        AnonymousModule() { Open("ABC") }
        |> produces
            """

open ABC

"""

    [<Test>]
    let ``Produces an open directive from a node`` () =
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
