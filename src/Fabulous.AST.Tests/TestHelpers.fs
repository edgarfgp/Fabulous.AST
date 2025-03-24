namespace Fabulous.AST.Tests

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Xunit
open VerifyXunit
open VerifyTests

[<AutoOpen>]
module TestHelpers =

    type VerifyBuilder(destination: string) =
        let settings destination =
            let settings = VerifySettings()
            settings.ScrubInlineGuids()
            settings.UseDirectory($"./snapshots/{destination}/")
            settings

        member this.Return<'T when 'T :> Oak>(source: WidgetBuilder<'T>) =
            let source = Gen.mkOak source |> Gen.run
            Verifier.Verify(source, settings destination).ToTask().Wait()

    let verify destination = VerifyBuilder(destination)

    let produces (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.mkOak source |> Gen.run
        Assert.Equal(expected.Trim(), res.Trim())

    let producesWithConfig (config: Fantomas.Core.FormatConfig) (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.mkOak source |> Gen.runWith config
        Assert.Equal(expected.Trim(), res.Trim())
