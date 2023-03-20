module Fabulous.AST.Tests

open FSharp.Compiler.Text
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Node

[<Test>]
let CanCompileBasicTree () =
    let source = Oak(){
        ModuleOrNamespace()
    }
        
    let compile = Tree.compile source
    let rootNode =
        compile
        |> unbox<Oak>
        |> CodeFormatter.FormatOakAsync
        |> Async.RunSynchronously
    
    Assert.NotNull(rootNode)
    
[<Test>]
let z () =
    let source =
        """
let x = 2
"""
    let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
    Assert.NotNull(rootNode)
