namespace Fabulous.AST.Tests.Namespaces

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module HashDirectives =
    [<Test>]
    let ``Produces an AnonymousModule with NoWarn directive`` () =
        (AnonymousModule() {
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attributes(AttributeNode "Obsolete")
        })
            .hashDirective(NoWarn("0044"))
        |> produces
            """
#nowarn "0044"
open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""

    [<Test>]
    let ``Produces an Namespace with NoWarn directive`` () =
        (Namespace("MyApp") {
            Open("System")

            (Record("HEX") {
                Field("R", CommonType.Int32)
                Field("G", CommonType.Int32)
                Field("B", CommonType.Int32)
            })
                .attributes(AttributeNode "Obsolete")
        })
            .hashDirective(NoWarn("0044"))
        |> produces
            """
#nowarn "0044"
namespace MyApp

open System

[<Obsolete>]
type HEX = { R: int; G: int; B: int }
"""
