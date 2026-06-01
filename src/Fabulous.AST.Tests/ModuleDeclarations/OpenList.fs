namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Open =

    [<Fact>]
    let ``Produces a simple open directive from a widget``() =
        Oak() { AnonymousModule() { Open("ABC") } }
        |> producesValid
            """
open ABC
"""

    [<Fact>]
    let ``Produces a simple open directive from a string list``() =
        Oak() {
            AnonymousModule() {
                Open([ "ABC"; "DEF" ])
                Open("ABC.DEF")
                Open("GHI")
            }

        }
        |> producesValid
            """

open ABC.DEF
open ABC.DEF
open GHI

"""

    [<Fact>]
    let ``Produces a simple open global directives from a string list``() =
        Oak() {
            AnonymousModule() {
                OpenGlobal([ "A" ])
                OpenGlobal("B")
                OpenGlobal([ "A"; "B" ])
            }

        }
        |> producesValid
            """
open global.A
open global.B
open global.A.B
"""

    [<Fact>]
    let ``Produces a simple open type directive from a string``() =
        Oak() {
            AnonymousModule() {
                OpenType([ "ABC"; "DFE" ])
                OpenType("ABC.DFE")
                OpenType("GHI")
            }
        }
        |> producesValid
            """

open type ABC.DFE
open type ABC.DFE
open type GHI

"""

    [<Fact>]
    let ``Produces a open and open type directives``() =
        Oak() {
            AnonymousModule() {
                Open("Fabulous.AST")
                OpenType([ "ABC"; "DFE" ])
            }
        }
        |> producesValid
            """

open Fabulous.AST
open type ABC.DFE

"""

    [<Fact>]
    let ``yield! a list of opens``() =
        Oak() { AnonymousModule() { yield! [ Open("Fabulous.AST"); OpenType([ "ABC"; "DFE" ]) ] } }
        |> producesValid
            """
open Fabulous.AST
open type ABC.DFE
"""
