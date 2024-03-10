namespace Fabulous.AST.Tests

open Fabulous.AST
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Xunit

[<AutoOpen>]
module TestHelpers =
    let produces (expected: string) (source: WidgetBuilder<#Oak>) =
        let oak = Gen.mkOak source

        let config =
            { FormatConfig.Default with
                InsertFinalNewline = false }

        let res = CodeFormatter.FormatOakAsync(oak, config) |> Async.RunSynchronously

        Assert.Equal(expected.Trim(), res.Trim())
