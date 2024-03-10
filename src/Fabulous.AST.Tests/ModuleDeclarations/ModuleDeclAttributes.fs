namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ModuleDeclAttributes =

    [<Fact>]
    let ``Produces a do expr`` () =
        AnonymousModule() {
            ModuleDeclAttribute(ConstantExpr("do printfn \"Executing...\"", false))
                .attributes() {
                Attribute "MyCustomModuleAttribute"
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Fact>]
    let ``Produces a ModuleDeclAttributes`` () =
        AnonymousModule() {
            ModuleDeclAttribute(ConstantExpr("do printfn \"Executing...\"", false))
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
