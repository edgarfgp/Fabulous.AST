namespace Fabulous.AST.Tests

open NUnit.Framework

open Fabulous.AST

open type Ast

module NodesTests =
    [<Test>]
    let CanCompileBasicTree () =
        Oak() {
            ModuleOrNamespace() {
                Let("hello", "\"World\"")
            }
        }
        |> produces "let hello = \"World\""
        
    [<Test>]
    let CanCompileBasicTree2 () =
        Oak() {
            ModuleOrNamespace() {
                Let("x", "123")
            }
        }
        |> produces "let x = 123"
