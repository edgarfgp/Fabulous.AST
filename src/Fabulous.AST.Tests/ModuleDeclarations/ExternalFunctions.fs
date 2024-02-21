namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module ExternalFunctions =

    [<Test>]
    let ``Produces an ExternBindingNodeNoParams`` () =
        AnonymousModule() { ExternBindingNode("HelloWorld", "void") }
        |> produces
            """
extern void HelloWorld()
"""

    [<Test>]
    let ``Produces an ExternBindingNode with parameter`` () =
        AnonymousModule() {
            ExternBindingNode("HelloWorld", "void")
                .parameter(ExternBindingPattern("string", ConstantPat("x")))
        }
        |> produces
            """
extern void HelloWorld(string x)
"""

    [<Test>]
    let ``Produces an ExternBindingNode with parameters`` () =
        AnonymousModule() {
            ExternBindingNode("HelloWorld", "void").parameters() {
                ExternBindingPattern("string", ConstantPat("x"))
                ExternBindingPattern(CommonType.mkLongIdent("int"), ConstantPat("y"))
            }
        }
        |> produces
            """
extern void HelloWorld(string x, int y)
"""
