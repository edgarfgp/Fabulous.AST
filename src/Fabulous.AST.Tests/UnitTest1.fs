module Fabulous.AST.Tests

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
        
   Assert.AreEqual(expected.Trim(), res.Trim())

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
let helloWorld () =
    Oak() {
        ModuleOrNamespace() {
            Let("x", "\"hello, world\"")
            Call("printfn", "\"%s\"", "x")
        }
    }
    |> check "
    
let x = \"hello, world\"
printfn \"%s\" x
    
"
    
[<Test>]
let z () =
    let source =
        """
printfn "Hello, world"
"""
    let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
    Assert.NotNull(rootNode)
