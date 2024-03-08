namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module ExternalFunctions =

    [<Test>]
    let ``Produces an ExternBindingNodeNoParams`` () =
        AnonymousModule() { ExternBinding("HelloWorld", "void") }
        |> produces
            """
extern void HelloWorld()
"""

    [<Test>]
    let ``Produces an ExternBindingNode with parameter`` () =
        AnonymousModule() {
            ExternBinding("HelloWorld", "void")
                .parameter(ExternBindingPat("string", ConstantPat("x")))
        }
        |> produces
            """
extern void HelloWorld(string x)
"""

    [<Test>]
    let ``Produces an ExternBindingNode with parameters`` () =
        AnonymousModule() {
            ExternBinding("HelloWorld", "void").parameters() {
                ExternBindingPat("string", ConstantPat("x"))
                ExternBindingPat(Int32(), ConstantPat("y"))
            }
        }
        |> produces
            """
extern void HelloWorld(string x, int y)
"""
