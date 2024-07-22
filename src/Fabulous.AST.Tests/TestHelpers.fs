namespace Fabulous.AST.Tests

open Fabulous.Builders
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fabulous.AST
open Xunit

[<AutoOpen>]
module TestHelpers =
    let produces (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.mkOak source |> Gen.run
        Assert.Equal(expected.Trim(), res.Trim())
