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
                Field("R", LongIdent("int"))
                Field("G", LongIdent("int"))
                Field("B", LongIdent("int"))
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
                Field("R", LongIdent("int"))
                Field("G", LongIdent("int"))
                Field("B", LongIdent("int"))
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
                Field("R", LongIdent("int"))
                Field("G", LongIdent("int"))
                Field("B", LongIdent("int"))
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
                Field("R", LongIdent("int"))
                Field("G", LongIdent("int"))
                Field("B", LongIdent("int"))
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
            Value("str", ConstantExpr(("Not debugging!")))
            HashDirective("else")
            Value("str", ConstantExpr("Debugging!"))
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
