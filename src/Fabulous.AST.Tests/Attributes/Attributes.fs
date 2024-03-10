namespace Fabulous.AST.Tests.Attributes

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AttributesNodes =

    [<Fact>]
    let ``Simple AttributeNode`` () =
        AnonymousModule() { Value("x", "12", false).attribute("Obsolete") }
        |> produces
            """
[<Obsolete>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode with expr`` () =
        AnonymousModule() {
            Value("x", "12", false)
                .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr("This is obsolete"))))
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Multiple attributes`` () =
        AnonymousModule() {
            Value("x", "12", false).attributes() { Attribute("Obsolete", ParenExpr(ConstantExpr("This is obsolete"))) }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode type name and target`` () =
        AnonymousModule() {
            Value("x", "12", false)
                .attribute(Attribute("Struct", "return"))
        }
        |> produces
            """
[<return: Struct>]
let x = 12
"""
