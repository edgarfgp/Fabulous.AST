namespace Fabulous.AST.Tests

open Fabulous.Builders
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fabulous.AST
open Xunit

[<AutoOpen>]
module TestHelpers =
    let produces (expected: string) (source: WidgetBuilder<#Oak>) =
        let oak = Gen.mkOak source

        let res =
            CodeFormatter.FormatOakAsync(oak, FormatConfig.Default)
            |> Async.RunSynchronously

        Assert.Equal(expected.Trim(), res.Trim())
