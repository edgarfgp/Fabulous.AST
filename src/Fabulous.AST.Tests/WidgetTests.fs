namespace Fabulous.AST.Tests

open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module WidgetTests =
    [<Test>]
    let ``Produces a top level let binding`` () =
        AnonymousModule() { Let("x", "12") }
        |> produces
            """
        
let x = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding`` () =
        AnonymousModule() {
            Let("x", "12").isMutable ()
        }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a simple hello world console app`` () =
        AnonymousModule() { Call("printfn", "\"hello, world\"") }
        |> produces
            """
        
printfn "hello, world"

"""

    [<Test>]
    let ``Produces Hello world with a let binding`` () =
        AnonymousModule() {
            Let("x", "\"hello, world\"")
            Call("printfn", "\"%s\"", "x")
        }
        |> produces
            """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Test>]
    let ``Produces several Call nodes`` () =
        AnonymousModule() {
            for i = 0 to 2 do
                Call("printfn", "\"%s\"", $"{i}")
        }
        |> produces
            """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""

    [<Test>]
    let ``Produces if-then`` () =
        AnonymousModule() { IfThen("x", "=", "12", Expr.For(Unit())) }
        |> produces
            """

if x = 12 then
    ()

"""

    [<Test>]
    let ``Produces FizzBuzz`` () =
        AnonymousModule() {
            Function(
                "fizzBuzz",
                [| "i" |],
                Match(Expr.For("i")) {
                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("15")), "=", Expr.For("0")),
                        Call("printfn", "\"FizzBuzz\"")
                    )

                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("5")), "=", Expr.For("0")),
                        Call("printfn", "\"Buzz\"")
                    )

                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("3")), "=", Expr.For("0")),
                        Call("printfn", "\"Fizz\"")
                    )

                    MatchClause("i", Call("printfn", "\"%i\"", "i"))
                }
            )
        }
        |> produces
            """

let fizzBuzz i =
    match i with
    | i when i % 15 = 0 -> printfn "FizzBuzz"
    | i when i % 5 = 0 -> printfn "Buzz"
    | i when i % 3 = 0 -> printfn "Fizz"
    | i -> printfn "%i" i

"""


    [<Test>]
    let ``Produces inlined FizzBuzz`` () =
        AnonymousModule() {
            Function(
                "fizzBuzz",
                [| "i" |],
                Match(Expr.For("i")) {
                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("15")), "=", Expr.For("0")),
                        Call("printfn", "\"FizzBuzz\"")
                    )

                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("5")), "=", Expr.For("0")),
                        Call("printfn", "\"Buzz\"")
                    )

                    MatchClause(
                        "i",
                        Condition(Condition(Expr.For("i"), "%", Expr.For("3")), "=", Expr.For("0")),
                        Call("printfn", "\"Fizz\"")
                    )

                    MatchClause("i", Call("printfn", "\"%i\"", "i"))
                }
            )
                .isInlined ()
        }
        |> produces
            """

let inline fizzBuzz i =
    match i with
    | i when i % 15 = 0 -> printfn "FizzBuzz"
    | i when i % 5 = 0 -> printfn "Buzz"
    | i when i % 3 = 0 -> printfn "Fizz"
    | i -> printfn "%i" i

"""

    [<Test>]
    let z () =
        let source =
            """

let mutable x = 10

"""

        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously
        Assert.NotNull(rootNode)
