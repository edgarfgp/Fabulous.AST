namespace Fabulous.AST.Tests.Core

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TriviaTests =

    [<Fact>]
    let ``Single line comment before``() =
        Oak() {
            AnonymousModule() { Value("x", ConstantExpr(Int(42)), "int").triviaBefore(SingleLine("This is a comment")) }
        }
        |> produces
            """
// This is a comment
let x: int = 42
"""

    [<Fact>]
    let ``Line comment after source code``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaAfter(LineCommentAfterSourceCode("This is a comment"))
            }
        }
        |> produces
            """
let x: int = 42 // This is a comment
"""

    [<Fact>]
    let ``Block comment without newlines``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaBefore(BlockComment("This is a block comment"))
            }
        }
        |> produces
            """
(*This is a block comment*) let x: int = 42
"""

    [<Fact>]
    let ``Block comment with newlines``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaBefore(BlockComment("This is a block comment", true, true))
            }
        }
        |> produces
            """
(*
This is a block comment
*)
let x: int = 42
"""

    [<Fact>]
    let ``Directive trivia``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Int(42)), "int").triviaBefore(Directive("#if DEBUG")) } }
        |> produces
            """
#if DEBUG
let x: int = 42
"""

    [<Fact>]
    let ``Newline trivia``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaAfter(Newline())
                Value("y", ConstantExpr(Int(43)), "int")
            }
        }
        |> produces
            """
let x: int = 42


let y: int = 43
"""

    [<Fact>]
    let ``Multiple trivia before``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaBefore([ SingleLine("First comment"); SingleLine("Second comment") ])
            }
        }
        |> produces
            """
// First comment
// Second comment
let x: int = 42
"""

    [<Fact>]
    let ``Multiple trivia after``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaAfter([ LineCommentAfterSourceCode("First comment"); Newline() ])
            }
        }
        |> produces
            """
let x: int = 42 // First comment

"""
