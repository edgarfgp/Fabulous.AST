namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module ModuleDeclAttributes =

    [<Test>]
    let ``Produces a do expr`` () =
        AnonymousModule() {
            ModuleDeclAttribute(ConstantExpr("do printfn \"Executing...\""))
                .attributes() {
                Attribute "MyCustomModuleAttribute"
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Test>]
    let ``Produces a ModuleDeclAttributes`` () =
        AnonymousModule() {
            ModuleDeclAttribute(ConstantExpr("do printfn \"Executing...\""))
                .attributes() {
                Attribute "MyCustomModuleAttribute"
                Attribute "MyCustomModuleAttribute2"
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute; MyCustomModuleAttribute2>]
do printfn "Executing..."
"""
