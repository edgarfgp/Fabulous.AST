namespace Fabulous.AST.Tests.Specialized

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

/// Real-world fixture tests — generate complete, realistic F# modules to validate
/// that the widget API composes well at file-scale, not just per-widget. Each test
/// produces a substantial chunk of F# code modeled on idiomatic code consumers
/// would actually write.
module RealWorld =

    [<Fact>]
    let ``Small Result/Option-style helper module``() =
        Oak() {
            AnonymousModule() {
                Module("Result") {
                    Function(
                        "map",
                        [ NamedPat("f"); NamedPat("r") ],
                        MatchExpr(
                            ConstantExpr(Constant "r"),
                            [ MatchClauseExpr(
                                  LongIdentPat("Ok", NamedPat("v")),
                                  AppExpr(
                                      ConstantExpr(Constant "Ok"),
                                      [ ParenExpr(AppExpr(ConstantExpr(Constant "f"), [ ConstantExpr(Constant "v") ])) ]
                                  )
                              )
                              MatchClauseExpr(
                                  LongIdentPat("Error", NamedPat("e")),
                                  AppExpr(ConstantExpr(Constant "Error"), [ ConstantExpr(Constant "e") ])
                              ) ]
                        )
                    )

                    Function(
                        "bind",
                        [ NamedPat("f"); NamedPat("r") ],
                        MatchExpr(
                            ConstantExpr(Constant "r"),
                            [ MatchClauseExpr(
                                  LongIdentPat("Ok", NamedPat("v")),
                                  AppExpr(ConstantExpr(Constant "f"), [ ConstantExpr(Constant "v") ])
                              )
                              MatchClauseExpr(
                                  LongIdentPat("Error", NamedPat("e")),
                                  AppExpr(ConstantExpr(Constant "Error"), [ ConstantExpr(Constant "e") ])
                              ) ]
                        )
                    )

                    Function(
                        "defaultValue",
                        [ NamedPat("d"); NamedPat("r") ],
                        MatchExpr(
                            ConstantExpr(Constant "r"),
                            [ MatchClauseExpr(LongIdentPat("Ok", NamedPat("v")), ConstantExpr(Constant "v"))
                              MatchClauseExpr(LongIdentPat("Error", WildPat()), ConstantExpr(Constant "d")) ]
                        )
                    )
                }
            }
        }
        |> producesValid
            """
module Result =
    let map f r =
        match r with
        | Ok v -> Ok (f v)
        | Error e -> Error e

    let bind f r =
        match r with
        | Ok v -> f v
        | Error e -> Error e

    let defaultValue d r =
        match r with
        | Ok v -> v
        | Error _ -> d
"""

    [<Fact>]
    let ``Domain types: discriminated union + record + helper functions``() =
        Oak() {
            Namespace("Shopping") {
                (Union("PaymentMethod") {
                    UnionCase("Cash")
                    UnionCase("Card", Field("last4", String()))
                    UnionCase("Voucher", [ Field("code", String()); Field("amount", Float()) ])
                })
                    .xmlDocs([ "How a customer paid." ])

                (Record("Order") {
                    Field("Id", LongIdent "System.Guid")
                    Field("Total", Float())
                    Field("Payment", LongIdent "PaymentMethod")
                })
                    .xmlDocs([ "A completed customer order." ])

                Module("Order") {
                    Function(
                        "isPaidInFull",
                        [ NamedPat("order") ],
                        MatchExpr(
                            ConstantExpr(Constant "order.Payment"),
                            [ MatchClauseExpr(LongIdentPat("Cash"), ConstantExpr(Bool true))
                              MatchClauseExpr(LongIdentPat("Card", WildPat()), ConstantExpr(Bool true))
                              MatchClauseExpr(
                                  LongIdentPat("Voucher", ParenPat(TuplePat([ WildPat(); NamedPat("amount") ]))),
                                  InfixAppExpr(
                                      ConstantExpr(Constant "amount"),
                                      ">=",
                                      ConstantExpr(Constant "order.Total")
                                  )
                              ) ]
                        )
                    )
                }
            }
        }
        |> producesValid
            """
namespace Shopping

/// How a customer paid.
type PaymentMethod =
    | Cash
    | Card of last4: string
    | Voucher of code: string * amount: float

/// A completed customer order.
type Order =
    { Id: System.Guid
      Total: float
      Payment: PaymentMethod }

module Order =
    let isPaidInFull order =
        match order.Payment with
        | Cash -> true
        | Card _ -> true
        | Voucher(_, amount) -> amount >= order.Total
"""

    [<Fact>]
    let ``Class implementing IDisposable with constructor parameter``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("FileHandle", ParenPat(ParameterPat("path", String()))) {
                    Member("this.Path", ConstantExpr(Constant "path"))

                    Member(
                        "this.Exists",
                        AppExpr(ConstantExpr(Constant "System.IO.File.Exists"), [ ConstantExpr(Constant "this.Path") ])
                    )

                    InterfaceWith(LongIdent "System.IDisposable") {
                        Member("this.Dispose", UnitPat(), ConstantExpr(Constant "()"))
                    }
                }
            }
        }
        |> producesValid
            """
type FileHandle(path: string) =
    member this.Path = path
    member this.Exists = System.IO.File.Exists this.Path

    interface System.IDisposable with
        member this.Dispose() = ()
"""
