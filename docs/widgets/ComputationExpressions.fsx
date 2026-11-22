(**
---
title: Computation Expressions
category: widgets
index: 20
---
*)

(**
# Computation Expressions

## Contents
- [Overview](#Overview)
- [Basic Usage](#Basic-Usage)
- [Named Computation Expressions](#Named-Computation-Expressions)
- [Multi-statement Bodies](#Multi-statement-Bodies)
- [Let and Use Bindings](#Let-and-Use-Bindings)
- [Bang Bindings](#Bang-Bindings)
- [Return and Yield](#Return-and-Yield)
- [For Loops](#For-Loops)
- [Query Expressions](#Query-Expressions)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Overview
A computation expression wraps a body in `{ ... }`. `ComputationExpr` produces an
anonymous block; `NamedComputationExpr` prefixes it with a builder name such as
`seq`, `async`, or `task`.

## Basic Usage
*)

Oak() { AnonymousModule() { Value("block", ComputationExpr(ConstantExpr(String "a"))) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Named Computation Expressions
Pass the builder name to `NamedComputationExpr`:
*)

Oak() {
    AnonymousModule() {
        Value("greeting", NamedComputationExpr("async", ConstantExpr(String "hello")))

        Value("work", NamedComputationExpr("task", ConstantExpr(String "done")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Multi-statement Bodies
Use `CompExprBodyExpr` to place several statements in the body:
*)

Oak() {
    AnonymousModule() {
        Value("numbers", NamedComputationExpr("seq", CompExprBodyExpr([ "yield 1"; "yield 2"; "yield 3" ])))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Let and Use Bindings
`CompExprBodyExpr` also takes statements: wrap a `Value` or `Use` binding in `LetOrUseExpr`
and any other expression in `OtherExpr`. This generates an `async` block that opens a
reader with `use`:
*)

Oak() {
    AnonymousModule() {
        Function(
            "readAll",
            ParenPat(ParameterPat("path", String())),
            NamedComputationExpr(
                "async",
                CompExprBodyExpr(
                    [ LetOrUseExpr(Use("reader", NewExpr("StreamReader", ParenExpr("path"))))
                      LetOrUseExpr(Value("text", "reader.ReadToEnd()"))
                      OtherExpr(SingleExpr(SingleNode("return", "text"))) ]
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Bang Bindings
`LetOrUseBangExpr` produces a `let!` binding and `AndBangExpr` an `and!` binding, so
independent awaits can run together. This generates a `task` that awaits two calls:
*)

Oak() {
    AnonymousModule() {
        Function(
            "fetchBalance",
            ParameterPat("client"),
            NamedComputationExpr(
                "task",
                CompExprBodyExpr(
                    [ LetOrUseBangExpr("orders", "client.GetOrdersAsync()")
                      AndBangExpr("refunds", "client.GetRefundsAsync()")
                      OtherExpr(SingleExpr(SingleNode("return", InfixAppExpr("orders", "-", "refunds")))) ]
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
`UseBangExpr` produces a `use!` binding, which disposes the bound value when it goes out of scope:
*)

Oak() {
    AnonymousModule() {
        Function(
            "readAll",
            ParameterPat("path", String()),
            NamedComputationExpr(
                "task",
                CompExprBodyExpr(
                    [ UseBangExpr("reader", "openReaderAsync path")
                      OtherExpr(SingleExpr("return!", "reader.ReadToEndAsync()")) ]
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Return and Yield
Keywords such as `return`, `return!`, `yield` and `yield!` are written with
`SingleExpr(SingleNode(keyword, expr))` (or the shorter `SingleExpr(keyword, expr)`). A space is
added after a keyword automatically; `SingleNode(...).addSpace(false)` turns it off. This generates a `seq` that yields a header line followed by
every row:
*)

Oak() {
    AnonymousModule() {
        Function(
            "report",
            [ ParameterPat("header"); ParameterPat("rows") ],
            SeqExpr(
                CompExprBodyExpr(
                    [ SingleExpr(SingleNode("yield", "header"))
                      SingleExpr(SingleNode("yield!", "rows")) ]
                )
            )
        )

        Function(
            "download",
            ParameterPat("url"),
            NamedComputationExpr("async", SingleExpr(SingleNode("return!", AppExpr("fetchAsync", "url"))))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## For Loops
`ForEachArrowExpr` produces the `for ... in ... -> ...` form used inside list, array and
sequence expressions, while `ForEachDoExpr` produces `for ... in ... do`. This generates a
list comprehension and a `seq` that yields from a loop:
*)

Oak() {
    AnonymousModule() {
        Value("squares", ListExpr([ ForEachArrowExpr("x", ConstantExpr("[ 1..5 ]"), InfixAppExpr("x", "*", "x")) ]))

        Value(
            "evens",
            SeqExpr(
                ForEachDoExpr(
                    "x",
                    "numbers",
                    IfThenExpr(
                        InfixAppExpr(InfixAppExpr("x", "%", Int(2)), "=", Int(0)),
                        SingleExpr(SingleNode("yield", "x"))
                    )
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Query Expressions
`JoinInExpr(lhs, rhs)` produces the `lhs in rhs` form used by the `join` operator of a
`query` expression. This generates a query that joins customers to their orders:
*)

Oak() {
    AnonymousModule() {
        Value(
            "orderTotals",
            NamedComputationExpr(
                "query",
                ForEachDoExpr(
                    "customer",
                    "customers",
                    CompExprBodyExpr(
                        [ JoinInExpr(
                              AppExpr("join", "order"),
                              AppExpr(
                                  "orders",
                                  [ ConstantExpr("on")
                                    ParenExpr(InfixAppExpr("customer.Id", "=", "order.CustomerId")) ]
                              )
                          )
                          AppExpr("select", ParenExpr(TupleExpr([ "customer.Name"; "order.Total" ]))) ]
                    )
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
