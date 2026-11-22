namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module OptionalVal =

    [<Fact>]
    let ``Optional parameter keeps its question mark``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Greeter", Constructor()) {
                    Member("this.Greet", ParameterPat(OptionalValPat("name"), String()), "name")
                    Member("this.Wave", ParameterPat(OptionalValPat("?name"), String()), "name")
                }
            }
        }
        |> producesValid
            """
type Greeter() =
    member this.Greet(?name: string) = name
    member this.Wave(?name: string) = name
"""
