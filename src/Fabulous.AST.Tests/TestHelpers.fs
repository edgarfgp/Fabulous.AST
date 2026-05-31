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
        Assert.Equal(expected.Trim().ReplaceLineEndings("\n"), res.Trim().ReplaceLineEndings("\n"))

    /// Like <c>produces</c>, but also round-trips the rendered source through the
    /// parser. When the output is not valid F#, <c>res</c> holds the parser
    /// diagnostics, so the assertion fails showing them against the expected source.
    let producesValid (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.mkOak source |> Gen.run |> Gen.parse
        Assert.Equal(expected.Trim().ReplaceLineEndings("\n"), res.Trim().ReplaceLineEndings("\n"))

    let producesWithConfig (config: Fantomas.Core.FormatConfig) (expected: string) (source: WidgetBuilder<#Oak>) =
        let res = Gen.run(Gen.mkOak source, config)
        Assert.Equal(expected.Trim().ReplaceLineEndings("\n"), res.Trim().ReplaceLineEndings("\n"))
