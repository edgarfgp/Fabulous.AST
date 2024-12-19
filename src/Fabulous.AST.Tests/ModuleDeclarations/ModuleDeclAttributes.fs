namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ModuleDeclAttributes =

    [<Fact>]
    let ``Produces a do expr AppExpr``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
                    .attribute(Attribute "MyCustomModuleAttribute")
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Fact>]
    let ``Produces a do expr Constant``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute(Constant(" printfn \"Executing...\""))
                    .attribute(Attribute "MyCustomModuleAttribute")
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Fact>]
    let ``Produces a do expr string``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute((" printfn \"Executing...\""))
                    .attribute(Attribute "MyCustomModuleAttribute")
            }
        }
        |> produces
            """
[<MyCustomModuleAttribute>]
do printfn "Executing..."
"""

    [<Fact>]
    let ``Produces a ModuleDeclAttributes AppExpr``() =
        Oak() {
            AnonymousModule() {
                ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
                    .attributes([ Attribute "MyCustomModuleAttribute"; Attribute "MyCustomModuleAttribute2" ])

            }
        }
        |> produces
            """
[<MyCustomModuleAttribute; MyCustomModuleAttribute2>]
do printfn "Executing..."
"""
