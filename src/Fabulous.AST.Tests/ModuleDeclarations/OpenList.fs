namespace Fabulous.AST.Tests.OpenDirectives

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Open =

    [<Fact>]
    let ``Produces a simple open directive from a widget``() =
        Oak() { AnonymousModule() { Open("ABC") } }
        |> produces
            """

open ABC

"""

    [<Fact>]
    let ``Produces a simple open directive from a string list``() =
        Oak() {
            AnonymousModule() {
                Open([ "ABC"; "."; "DEF" ])
                Open("GHI")
            }
        }
        |> produces
            """

open ABC.DEF
open GHI

"""

    [<Fact>]
    let ``Produces a simple open type directive from a string``() =
        Oak() {
            AnonymousModule() {
                OpenType([ LongIdent("ABC"); LongIdent("DFE"); String() ])
                OpenType("GHI")
            }
        }
        |> produces
            """

open type ABC
open type DFE
open type string

open type GHI

"""

    [<Fact>]
    let ``Produces a open and open type directives``() =
        Oak() {
            AnonymousModule() {
                Open("Fabulous.AST")
                OpenType([ (LongIdent "ABC"); LongIdent("DFE"); String() ])
            }
        }
        |> produces
            """

open Fabulous.AST

open type ABC
open type DFE
open type string

"""
