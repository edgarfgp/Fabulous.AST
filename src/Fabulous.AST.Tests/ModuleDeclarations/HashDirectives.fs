namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module HashDirectives =
    [<Fact>]
    let ``Produces an AnonymousModule with NoWarn directive``() =
        Oak() {
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
        }
        |> produces
            """
#nowarn "0044"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple NoWarn directive``() =
        Oak() {
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
        }
        |> produces
            """
#nowarn "0044" "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple line NoWarn directive``() =
        Oak() {
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
        }
        |> produces
            """
#nowarn "0044"
#nowarn "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an Namespace with NoWarn directive``() =
        Oak() {
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
        }

        |> produces
            """
namespace MyApp

#nowarn "0044"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an Namespace with Conditional directive``() =
        Oak() {
            AnonymousModule() {
                HashDirective("if", "!DEBUG")
                Value("str", ConstantExpr(DoubleQuoted "Not debugging!"))
                HashDirective("else")
                Value("str", ConstantExpr(DoubleQuoted "Debugging!"))
                HashDirective("endif")
            }
        }
        |> produces
            """
#if !DEBUG
let str = "Not debugging!"
#else
let str = "Debugging!"
#endif
"""
