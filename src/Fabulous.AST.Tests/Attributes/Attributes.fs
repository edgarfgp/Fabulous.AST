namespace Fabulous.AST.Tests.Attributes

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AttributesNodes =

    [<Fact>]
    let ``Simple AttributeNode``() =
        Oak() { AnonymousModule() { Value("x", "12").hasQuotes(false).attribute("Obsolete") } }
        |> produces
            """
[<Obsolete>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode with expr``() =
        Oak() {
            AnonymousModule() {
                Value("x", "12")
                    .hasQuotes(false)
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr("This is obsolete"))))
            }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Multiple attributes``() =
        Oak() {
            AnonymousModule() {
                Value("x", "12").hasQuotes(false).attributes() {
                    Attribute("Obsolete", ParenExpr(ConstantExpr("This is obsolete")))
                }
            }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode type name and target``() =
        Oak() { AnonymousModule() { Value("x", "12").hasQuotes(false).attribute(Attribute("Struct", "return")) } }
        |> produces
            """
[<return: Struct>]
let x = 12
"""
