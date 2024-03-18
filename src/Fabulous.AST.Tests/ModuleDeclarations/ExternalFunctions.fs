namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ExternalFunctions =

    [<Fact>]
    let ``Produces an ExternBindingNodeNoParams``() =
        Oak() { AnonymousModule() { ExternBinding("HelloWorld", "void") } }
        |> produces
            """
extern void HelloWorld()
"""

    [<Fact>]
    let ``Produces an ExternBindingNode with parameter``() =
        Oak() {
            AnonymousModule() {
                ExternBinding("HelloWorld", "void")
                    .parameter(ExternBindingPat("string", ConstantPat("x")))
            }
        }
        |> produces
            """
extern void HelloWorld(string x)
"""

    [<Fact>]
    let ``Produces an ExternBindingNode with parameters``() =
        Oak() {
            AnonymousModule() {
                ExternBinding("HelloWorld", "void").parameters() {
                    ExternBindingPat("string", ConstantPat("x"))
                    ExternBindingPat(Int32(), ConstantPat("y"))
                }
            }
        }
        |> produces
            """
extern void HelloWorld(string x, int y)
"""
