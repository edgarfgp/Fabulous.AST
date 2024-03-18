namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module OpenType =

    [<Fact>]
    let ``Produces a simple open type directive from a string``() =
        Oak() { AnonymousModule() { OpenType("ABC") } }
        |> produces
            """

open type ABC

"""
