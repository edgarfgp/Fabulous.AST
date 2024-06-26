namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ModuleDeclAttributes =

    [<Fact>]
    let ``Produces a do expr``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute(
                    ConstantExpr(Constant "do printfn \"Executing...\""),
                    Attribute("MyCustomModuleAttribute")
                )
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Fact>]
    let ``Produces a ModuleDeclAttributes``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute(
                    ConstantExpr(Constant "do printfn \"Executing...\""),
                    [ Attribute "MyCustomModuleAttribute"; Attribute "MyCustomModuleAttribute2" ]
                )

            }
        }
        |> produces
            """
[<MyCustomModuleAttribute; MyCustomModuleAttribute2>]
do printfn "Executing..."
"""
