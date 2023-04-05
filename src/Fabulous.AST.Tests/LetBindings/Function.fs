namespace Fabulous.AST.Tests.LetBindings

open Fabulous.AST

open type Ast

module Function =
    ()
//     [<Test>]
//     let ``Produces if-then`` () =
//         AnonymousModule() { IfThen("x", "=", "12", Expr.For(Unit())) }
//         |> produces
//             """
//
// if x = 12 then
//     ()
//
// """

//     [<Test>]
//     let ``Produces FizzBuzz`` () =
//         AnonymousModule() {
//             Function("fizzBuzz", [| "i" |]) {
//                 Match(Expr.For("i")) {
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("15")), "=", Expr.For("0")),
//                         Call("printfn", "\"FizzBuzz\"")
//                     )
//
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("5")), "=", Expr.For("0")),
//                         Call("printfn", "\"Buzz\"")
//                     )
//
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("3")), "=", Expr.For("0")),
//                         Call("printfn", "\"Fizz\"")
//                     )
//
//                     MatchClause("i", Call("printfn", "\"%i\"", "i"))
//                 }
//             }
//         }
//         |> produces
//             """
//
// let fizzBuzz i =
//     match i with
//     | i when i % 15 = 0 -> printfn "FizzBuzz"
//     | i when i % 5 = 0 -> printfn "Buzz"
//     | i when i % 3 = 0 -> printfn "Fizz"
//     | i -> printfn "%i" i
//
// """

//     [<Test>]
//     let ``Produces inlined FizzBuzz`` () =
//         AnonymousModule() {
//             Function("fizzBuzz", [| "i" |]).isInlined() {
//                 Match(Expr.For("i")) {
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("15")), "=", Expr.For("0")),
//                         Call("printfn", "\"FizzBuzz\"")
//                     )
//
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("5")), "=", Expr.For("0")),
//                         Call("printfn", "\"Buzz\"")
//                     )
//
//                     MatchClause(
//                         "i",
//                         Condition(Condition(Expr.For("i"), "%", Expr.For("3")), "=", Expr.For("0")),
//                         Call("printfn", "\"Fizz\"")
//                     )
//
//                     MatchClause("i", Call("printfn", "\"%i\"", "i"))
//                 }
//             }
//         }
//         |> produces
//             """
//
// let inline fizzBuzz i =
//     match i with
//     | i when i % 15 = 0 -> printfn "FizzBuzz"
//     | i when i % 5 = 0 -> printfn "Buzz"
//     | i when i % 3 = 0 -> printfn "Fizz"
//     | i -> printfn "%i" i
//
// """
