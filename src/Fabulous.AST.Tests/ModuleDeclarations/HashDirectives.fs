namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module HashDirectives =
    [<Test>]
    let ``Produces an AnonymousModule with NoWarn directive`` () =
        AnonymousModule() {
            NoWarn("0044")
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attribute(Attribute "Obsolete")
        }
        |> produces
            """
#nowarn "0044"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Test>]
    let ``Produces an AnonymousModule with multiple NoWarn directive`` () =
        AnonymousModule() {
            NoWarn([ "0044"; "0045" ])
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attribute("Obsolete")
        }
        |> produces
            """
#nowarn "0044" "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Test>]
    let ``Produces an AnonymousModule with multiple line NoWarn directive`` () =
        AnonymousModule() {
            NoWarn("0044")
            NoWarn("0045")
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attribute("Obsolete")
        }
        |> produces
            """
#nowarn "0044"
#nowarn "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Test>]
    let ``Produces an Namespace with NoWarn directive`` () =
        Namespace("MyApp") {
            NoWarn("0044")
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attribute(Attribute "Obsolete")
        }
        |> produces
            """
namespace MyApp

#nowarn "0044"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Test>]
    let ``Produces an Namespace with Conditional directive`` () =
        AnonymousModule() {
            HashDirective("if", "!DEBUG")
            Value("str", ConstantExpr(ConstantString("\"Not debugging!\"")))
            HashDirective("else")
            Value("str", ConstantExpr(ConstantString("\"Debugging!\"")))
            HashDirective("endif")
        }
        |> produces
            """
#if !DEBUG
let str = "Not debugging!"
#else
let str = "Debugging!"
#endif
"""
