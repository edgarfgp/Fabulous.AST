namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TryWith =
    [<Fact>]
    let ``Basic TryWith expressions``() =
        Oak() {
            AnonymousModule() {
                TryWithExpr(Int(12), [])

                TryWithExpr(
                    ConstantExpr(Int(12)),
                    [ MatchClauseExpr("a", AppExpr("failwith", String("Not implemented"))) ]
                )

                TryWithExpr(
                    Int(12),
                    [ MatchClauseExpr("b", FailWithExpr(String("Not implemented")))
                      MatchClauseExpr(WildPat(), FailWithExpr("Not implemented")) ]
                )
            }
        }
        |> produces
            """
try
    12
with


try
    12
with
| a -> failwith "Not implemented"

try
    12
with
| b -> failwith "Not implemented"
| _ -> failwith "Not implemented"
"""

    [<Fact>]
    let ``TryWith with different value types``() =
        Oak() {
            AnonymousModule() {
                // WidgetBuilder<Expr> value
                TryWithExpr(AppExpr("compute", Int(10)), [ MatchClauseExpr("ex", ConstantExpr(Int(-1))) ])

                // WidgetBuilder<Constant> value
                TryWithExpr(Int(20), [ MatchClauseExpr("ex", ConstantExpr(Int(-1))) ])

                // string value
                TryWithExpr("computeValue()", [ MatchClauseExpr("ex", ConstantExpr(Int(-1))) ])
            }
        }
        |> produces
            """
try
    compute 10
with
| ex -> -1

try
    20
with
| ex -> -1

try
    computeValue()
with
| ex -> -1
"""

    [<Fact>]
    let ``TryWith with single exception handler``() =
        Oak() {
            AnonymousModule() {
                // WidgetBuilder<Expr>, exception name, handler
                TryWithExpr(AppExpr("parseInput", String("input")), "ex", ConstantExpr(Int(-1)))

                // WidgetBuilder<Constant>, exception name, handler
                TryWithExpr(Int(30), "ex", ConstantExpr(Int(-1)))

                // string, exception name, handler
                TryWithExpr("computeValue()", "ex", ConstantExpr(Int(-1)))
            }
        }
        |> produces
            """
try
    parseInput "input"
with
| ex -> -1

try
    30
with
| ex -> -1

try
    computeValue()
with
| ex -> -1
"""

    [<Fact>]
    let ``TryWith with specific exception types``() =
        Oak() {
            AnonymousModule() {
                // Using custom match clauses to test type-specific exception handling
                TryWithExpr(
                    AppExpr("parseInput", String("input")),
                    [ MatchClauseExpr(
                          AsPat(IsInstPat("System.ArgumentException"), "argEx"),
                          AppExpr("printfn", String("Argument error: %s"))
                      ) ]
                )

                TryWithExpr(
                    Int(40),
                    [ MatchClauseExpr(AsPat(IsInstPat("System.DivideByZeroException"), "divEx"), ConstantExpr(Int(0))) ]
                )

                TryWithExpr(
                    "computeValue()",
                    [ MatchClauseExpr(
                          AsPat(IsInstPat("System.InvalidOperationException"), "opEx"),
                          ConstantExpr(String("Operation failed"))
                      ) ]
                )
            }
        }
        |> produces
            """
try
    parseInput "input"
with
| :? System.ArgumentException as argEx -> printfn "Argument error: %s"

try
    40
with
| :? System.DivideByZeroException as divEx -> 0

try
    computeValue()
with
| :? System.InvalidOperationException as opEx -> "Operation failed"
"""

    [<Fact>]
    let ``Complex TryWith expressions``() =
        Oak() {
            AnonymousModule() {
                // Nested try-with
                TryWithExpr(
                    TryWithExpr(AppExpr("parseInput", String("input")), "innerEx", AppExpr("logError", "innerEx")),
                    "outerEx",
                    AppExpr("logError", "outerEx")
                )

                // Multiple catch clauses with different exception types
                TryWithExpr(
                    AppExpr("processData", "data"),
                    [ MatchClauseExpr(
                          AsPat(IsInstPat("System.ArgumentException"), "argEx"),
                          AppExpr("handleArgumentError", "argEx")
                      )

                      MatchClauseExpr(
                          AsPat(IsInstPat("System.InvalidOperationException"), "opEx"),
                          AppExpr("handleOperationError", "opEx")
                      )

                      MatchClauseExpr(NamedPat("ex"), AppExpr("handleGenericError", "ex")) ]
                )
            }
        }
        |> produces
            """
try
    try
        parseInput "input"
    with
    | innerEx -> logError innerEx
with
| outerEx -> logError outerEx

try
    processData data
with
| :? System.ArgumentException as argEx -> handleArgumentError argEx
| :? System.InvalidOperationException as opEx -> handleOperationError opEx
| ex -> handleGenericError ex
"""
