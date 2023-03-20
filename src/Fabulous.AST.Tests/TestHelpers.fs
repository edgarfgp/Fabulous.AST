namespace Fabulous.AST.Tests

open Fabulous.AST
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

[<AutoOpen>]
module TestHelpers =
    let produces (expected: string) (source: WidgetBuilder<#IFabOak>)=
        let oak =  Tree.compile source |> unbox<Oak>
        
        let config =
            { FormatConfig.Default with
                InsertFinalNewline = false }
            
        let res =
            CodeFormatter.FormatOakAsync(oak, config)
            |> Async.RunSynchronously
            
        Assert.AreEqual(expected.Trim(), res.Trim())