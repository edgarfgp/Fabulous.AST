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
                        ParenType("^N"),
                        SigMember(ValField([ "static"; "member" ], "Bar", Funs("_", "_"))),
                        ConstantExpr("source")
                    )
                )

                ParenExpr(
                    TraitCallExpr(
                        "(^N)",
                        SigMember(ValField([ "static"; "member" ], "Bar", Funs("_", "_"))),
                        ConstantExpr("source")
                    )
                )

                ParenExpr(
                    TraitCallExpr("(^N)", SigMember(ValField([ "static"; "member" ], "Bar", Funs("_", "_"))), "source")
                )
            }
        }
        |> produces
            """
((^N): (static member Bar: _ -> _) source)
((^N): (static member Bar: _ -> _) source)
((^N): (static member Bar: _ -> _) source)
"""
