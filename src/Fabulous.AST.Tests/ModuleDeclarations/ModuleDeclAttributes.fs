namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module ModuleDeclAttributes =

    [<Test>]
    let ``Produces a do expr`` () =
        AnonymousModule() {
            ModuleDeclAttributeNode(
                ConstantExpr(ConstantString("do printfn \"Executing...\"")),
                AttributeNode "MyCustomModuleAttribute"
            )
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Test>]
    let ``Produces a ModuleDeclAttributes`` () =
        AnonymousModule() {
            ModuleDeclAttributeNode(
                ConstantExpr(ConstantString("do printfn \"Executing...\"")),
                AttributeNodes() {
                    AttributeNode "MyCustomModuleAttribute"
                    AttributeNode "MyCustomModuleAttribute2"
                }
            )
        }
        |> produces
            """
[<MyCustomModuleAttribute; MyCustomModuleAttribute2>]
do printfn "Executing..."
"""
