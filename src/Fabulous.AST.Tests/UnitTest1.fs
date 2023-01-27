module Fabulous.AST.Tests

open NUnit.Framework

open Fabulous.AST
open type Fabulous.AST.Node

[<Test>]
let CanCompileBasicTree () =
    let tree =
        ModuleOrNamespace(
            Range("test.fs", Position(1, 2), Position(3, 4))
        )
        
    let rootNode = Tree.compile tree :?> Fantomas.Core.SyntaxOak.ModuleOrNamespaceNode
    
    Assert.NotNull(rootNode.Range)
    Assert.AreEqual(rootNode.Range.FileName, "test.fs")
    Assert.AreEqual(rootNode.Range.Start.Line, 1)
    Assert.AreEqual(rootNode.Range.Start.Column, 2)
    Assert.AreEqual(rootNode.Range.End.Line, 3)
    Assert.AreEqual(rootNode.Range.End.Column, 4)
