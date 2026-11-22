namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ExternalFunctions =

    [<Fact>]
    let ``Produces an ExternBindingNodeNoParams``() =
        Oak() { AnonymousModule() { ExternBinding(LongIdent("void"), "HelloWorld") } }
        |> producesValid
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
        |> producesValid
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
        |> producesValid
            """
extern void HelloWorld(string x, int y)
"""

    [<Fact>]
    let ``Produces an ExternBindingNode with return type attributes``() =
        Oak() {
            AnonymousModule() {
                ExternBinding("bool", "CloseHandle", ExternBindingPat("nativeint", "handle"))
                    .attribute(Attribute("DllImport", ParenExpr(String("kernel32.dll"))))
                    .returnAttribute(Attribute("MarshalAs", ParenExpr(Constant("UnmanagedType.Bool"))))

                ExternBinding("int", "GetValue").returnAttributes([ Attribute("A"); Attribute("B") ])
            }
        }
        |> producesValid
            """
[<DllImport("kernel32.dll")>]
extern [<MarshalAs(UnmanagedType.Bool)>] bool CloseHandle(nativeint handle)

extern [<A; B>] int GetValue()
"""
