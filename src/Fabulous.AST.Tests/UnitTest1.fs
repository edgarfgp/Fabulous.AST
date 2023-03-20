module Fabulous.AST.Tests

open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Node

let check (expected: string) (source: WidgetBuilder<IFabOak>)=
   let res =
       Tree.compile source
        |> unbox<Oak>
        |> CodeFormatter.FormatOakAsync
        |> Async.RunSynchronously
        
   Assert.AreEqual(expected, res)

[<Test>]
let CanCompileBasicTree () =
    ast {
        Let("hello", "\"World\"")
    }
    |> check "let hello = \"World\"\n"
    
[<Test>]
let CanCompileBasicTree2 () =
    Oak() {
        ModuleOrNamespace() {
            Let("x", "123")
        }
    }
    |> check "let x = 123\n"
    
[<Test>]
let z () =
    let source =
        """
let x = 12
"""
    let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
    Assert.NotNull(rootNode)
