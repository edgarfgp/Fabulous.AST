namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module OpenType =

    [<Fact>]
    let ``Produces a simple open type directive from a string``() =
        Oak() { AnonymousModule() { OpenType(LongIdent "ABC") } }
        |> produces
            """

open type ABC

"""
