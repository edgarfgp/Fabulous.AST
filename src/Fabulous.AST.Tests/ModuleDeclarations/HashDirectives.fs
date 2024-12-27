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
                NoWarn(String "0044")
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
    let ``Produces an AnonymousModule with Help directive``() =
        Oak() {
            AnonymousModule() {
                Help("List.map")
                Help(String("List.map"))
            }
        }
        |> produces
            """
#help List.map
#help "List.map"
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple string NoWarn directive``() =
        Oak() {
            AnonymousModule() {
                NoWarn([ String "0044"; String "0045" ])
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
#nowarn "0044" "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple non string NoWarn directive``() =
        Oak() {
            AnonymousModule() {
                NoWarn([ Int 0044; Int 0045 ])
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
#nowarn 44 45
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple line NoWarn directive``() =
        Oak() {
            AnonymousModule() {
                NoWarn(String "0044")
                NoWarn(String "0045")
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
#nowarn "0045"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an AnonymousModule with multiple line non string NoWarn directive``() =
        Oak() {
            AnonymousModule() {
                NoWarn(Int 44)
                NoWarn(Int 45)
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
#nowarn 44
#nowarn 45
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Fact>]
    let ``Produces an Namespace with NoWarn directive``() =
        Oak() {
            Namespace("MyApp") {
                NoWarn(String "0044")
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
                Value(ConstantPat(Constant("str")), ConstantExpr(String("Not debugging!")))
                HashDirective("else")
                Value(ConstantPat(Constant("str")), ConstantExpr(String("Debugging!")))
                HashDirective("endif")
                HashDirective("help", [ String "List.map" ])
                HashDirective("help", [ "List.map" ])
                HashDirective("help")
            }
        }
        |> produces
            """
#if !DEBUG
let str = "Not debugging!"
#else
let str = "Debugging!"
#endif
#help "List.map"
#help List.map
#help
"""
