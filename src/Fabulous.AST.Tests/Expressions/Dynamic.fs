namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Dynamic =
    [<Fact>]
    let ``Dynamic expression``() =
        Oak() {
            AnonymousModule() {
                DynamicExpr(Int(12), Int(12))

                DynamicExpr(ConstantExpr(Int(12)), AppExpr("failwith", String("Not implemented")))

                DynamicExpr(Int(12), FailWithExpr(String("Not implemented")))
            }
        }
        |> produces
            """
12?12
12?failwith "Not implemented"
12?failwith "Not implemented"
"""

    [<Fact>]
    let ``Dynamic expression with string value and Constant arg``() =
        Oak() { AnonymousModule() { DynamicExpr("obj", Int(42)) } }
        |> produces
            """
obj?42
"""

    [<Fact>]
    let ``Dynamic chain keeps arguments tight``() =
        Oak() {
            AnonymousModule() {
                Value("a", DynamicChainExpr("x", [ DynamicMember("a"); DynamicMember("b", Int(1)) ]))

                Value(
                    "b",
                    DynamicChainExpr("x", [ DynamicMember("a", ConstantExpr(ConstantUnit())); DynamicMember("b") ])
                )

                Value(
                    "c",
                    DynamicChainExpr(
                        ParenExpr(AppExpr("f", "x")),
                        [ DynamicMember("a", TupleExpr([ "1"; "2" ])); DynamicMember("b") ]
                    )
                )
            }
        }
        |> producesValid
            """
let a = x?a?b(1)
let b = x?a()?b
let c = (f x)?a(1, 2)?b
"""

    [<Fact>]
    let ``Dynamic chain parenthesizes a non-atomic leading expression and escapes keyword members``() =
        Oak() {
            AnonymousModule() {
                Value("a", DynamicChainExpr(AppExpr("f", "x"), [ DynamicMember("a"); DynamicMember("b") ]))
                Value("b", DynamicChainExpr(PrefixAppExpr("-", "x"), [ DynamicMember("a") ]))
                Value("c", DynamicChainExpr("x", [ DynamicMember("type"); DynamicMember("my member", Int(1)) ]))
            }
        }
        |> producesValid
            """
let a = (f x)?a?b
let b = (-x)?a
let c = x?``type``?``my member``(1)
"""

    [<Fact>]
    let ``Dynamic chain needs at least one member``() =
        Assert.Throws<System.ArgumentException>(fun () ->
            Oak() { AnonymousModule() { DynamicChainExpr("x", []) } }
            |> Gen.mkOak
            |> Gen.run
            |> ignore)
        |> ignore

    [<Fact>]
    let ``Dynamic expression escapes keyword member names``() =
        Oak() {
            AnonymousModule() {
                Value("a", DynamicExpr("obj", "type"))
                Value("b", DynamicExpr("obj", "Name"))
                // Other strings are still raw expression text.
                Value("c", DynamicExpr("obj", "f y"))
            }
        }
        |> producesValid
            """
let a = obj?``type``
let b = obj?Name
let c = obj?f y
"""
