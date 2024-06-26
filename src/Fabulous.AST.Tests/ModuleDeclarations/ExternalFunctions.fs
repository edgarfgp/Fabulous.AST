namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ExternalFunctions =

    [<Fact>]
    let ``Produces an ExternBindingNodeNoParams``() =
        Oak() { AnonymousModule() { ExternBinding(LongIdent("void"), "HelloWorld") } }
        |> produces
            """
extern void HelloWorld()
"""

    [<Fact>]
    let ``Produces an ExternBindingNode with parameter``() =
        Oak() {
            AnonymousModule() {
                ExternBinding(
                    LongIdent("void"),
                    "HelloWorld",
                    ExternBindingPat(LongIdent "string", ConstantPat(Constant "x"))
                )
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
                ExternBinding(
                    LongIdent "void",
                    "HelloWorld",
                    [ ExternBindingPat(LongIdent "string", ConstantPat(Constant "x"))
                      ExternBindingPat(Int(), ConstantPat(Constant "y")) ]
                )
            }
        }
        |> produces
            """
extern void HelloWorld(string x, int y)
"""
