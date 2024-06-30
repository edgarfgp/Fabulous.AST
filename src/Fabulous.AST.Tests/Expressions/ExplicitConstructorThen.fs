namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ExplicitConstructorThen =

    [<Fact>]
    let ``let value with a ExplicitConstructorThen expression``() =
        Oak() {
            AnonymousModule() {
                Class("MyClass", ImplicitConstructor(ParenPat(TuplePat([ "x0"; "y0"; "z0" ])))) {
                    ExplicitConstructor(
                        ParenPat(""),
                        CompExprBodyExpr(
                            [ AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ "0"; "0"; "0" ])))
                              ExplicitConstructorThenExpr(
                                  AppExpr("printfn", String("Created an invalid MyClass object."))
                              ) ]
                        )
                    )
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    new() =
        MyClass (0, 0, 0)
        then printfn "Created an invalid MyClass object."
"""
