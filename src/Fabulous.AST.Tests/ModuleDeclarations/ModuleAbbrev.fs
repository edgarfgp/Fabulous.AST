namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ModuleAbbrev =

    [<Fact>]
    let ``Produces a ModuleAbbrev``() =
        Oak() { AnonymousModule() { ModuleAbbrev("SizeType", "()") } }

        |> produces
            """

module SizeType = ()

"""
