namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TraitCall =

    [<Fact>]
    let ``let value with a TraitCall expression``() =
        Oak() {
            AnonymousModule() {
                ParenExpr(
                    TraitCallExpr(
                        Paren("^N"),
                        SigMember(Val([ "static"; "member" ], "Bar", Funs("_", "_"))),
                        ConstantExpr("source")
                    )
                )

                ParenExpr(
                    TraitCallExpr(
                        "(^N)",
                        SigMember(Val([ "static"; "member" ], "Bar", Funs("_", "_"))),
                        ConstantExpr("source")
                    )
                )

                ParenExpr(
                    TraitCallExpr("(^N)", SigMember(Val([ "static"; "member" ], "Bar", Funs("_", "_"))), "source")
                )
            }
        }
        |> produces
            """
((^N): (static member Bar: _ -> _) source)
((^N): (static member Bar: _ -> _) source)
((^N): (static member Bar: _ -> _) source)
"""

    [<Fact>]
    let ``let value with a TraitCall expression OrType``() =
        Oak() {
            AnonymousModule() {
                ParenExpr(
                    TraitCallExpr(
                        Paren(Or("^I", "^R")),
                        SigMember(Val([ "static"; "member" ], "Map", Funs("^R", Tuple([ "^I"; "^F" ])))),
                        TupleExpr([ "source"; "mapping" ])
                    )
                )
            }
        }
        |> produces
            """
((^I or ^R): (static member Map: ^I * ^F -> ^R) source, mapping)
"""
