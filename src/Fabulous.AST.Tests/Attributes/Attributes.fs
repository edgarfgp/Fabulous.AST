namespace Fabulous.AST.Tests.Attributes

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AttributesNodes =

    [<Fact>]
    let ``Simple AttributeNode``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").attribute("Obsolete") } }
        |> produces
            """
[<Obsolete>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode with expr``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12")
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(DoubleQuoted "This is obsolete"))))
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
                Value("x", Unquoted "12")
                    .attributes([ Attribute("Obsolete", ParenExpr(ConstantExpr(DoubleQuoted "This is obsolete"))) ])
            }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode type name and target``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").attribute(Attribute("Struct", "return")) } }
        |> produces
            """
[<return: Struct>]
let x = 12
"""
