module Fabulous.AST.Tests

open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Node

let check (expected: string) (source: WidgetBuilder<IFabOak>)=
   let oak =  Tree.compile source |> unbox<Oak>
   let config =
        { FormatConfig.Default with
            InsertFinalNewline = false }       
   let res =
       CodeFormatter.FormatOakAsync(oak, config)
        |> Async.RunSynchronously
        
   Assert.AreEqual(expected.Trim(), res.Trim())

[<Test>]
let CanCompileBasicTree () =
    ast {
        Let("hello", "World")
    }
    |> check "let hello = World"
    
[<Test>]
let CanCompileBasicTree2 () =
    Oak() {
        ModuleOrNamespace() {
            Let("x", "123")
        }
    }
    |> check "let x = 123"
    
[<Test>]
let helloWorld () =
    Oak() {
        ModuleOrNamespace() {
            Let("x", "\"hello, world\"")
            Call("printfn", "\"%s\"", "x")
        }
    }
    |> check """
let x = "hello, world"
printfn "%s" x
"""

[<Test>]
let inspectOak () =
    let source =
        """
printfn "Hello, world"
"""
    let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
    Assert.NotNull(rootNode)
