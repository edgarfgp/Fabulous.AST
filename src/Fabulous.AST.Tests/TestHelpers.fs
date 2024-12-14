namespace Fabulous.AST.Tests

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Xunit

[<AutoOpen>]
module TestHelpers =
    let produces (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.mkOak source |> Gen.run
        Assert.Equal(expected.Trim(), res.Trim())
