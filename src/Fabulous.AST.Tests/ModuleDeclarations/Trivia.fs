namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Trivia =
    [<Fact>]
    let ``Produces line comment before``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaBefore(SingleLine("Comment before"))

            }
        }
        |> producesValid
            """
// Comment before
let x = 10
"""

    [<Fact>]
    let ``Produces line comment before with multiple values``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaBefore([ SingleLine("Comment before"); SingleLine("Another comment") ])
            }
        }
        |> producesValid
            """
// Comment before
// Another comment
let x = 10
"""

    [<Fact>]
    let ``Produces line comment before with a new line before``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaBefore([ Newline(); SingleLine("Comment before") ]) } }
        |> producesValid
            """

// Comment before
let x = 10
"""

    [<Fact>]
    let ``Produces line comment before with a new line after``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaBefore([ SingleLine("Comment before"); Newline() ]) } }
        |> producesValid
            """
// Comment before

let x = 10
"""

    [<Fact>]
    let ``Produces line comment before with a new line before and after``() =
        Oak() {
            AnonymousModule() { Value("x", "10").triviaBefore([ Newline(); SingleLine("Comment before"); Newline() ]) }
        }
        |> producesValid
            """

// Comment before

let x = 10
"""

    [<Fact>]
    let ``Produces line comment after source code``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaBefore([ LineCommentAfterSourceCode("Comment after source code") ])
            }
        }
        |> produces
            """
let x =// Comment after source code
    10
"""
    (*Comment before*)
    [<Fact>]
    let ``Produces block comment before``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaBefore([ BlockComment("Comment before") ]) } }
        |> producesValid
            """
(*Comment before*) let x = 10
"""

    [<Fact>]
    let ``Produces block comment before with new line before``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaBefore([ BlockComment("Comment before", newlineBefore = true) ])
            }
        }
        |> producesValid
            """
(*
Comment before*) let x = 10
"""

    [<Fact>]
    let ``Produces block comment before with new line after``() =
        Oak() {
            AnonymousModule() { Value("x", "10").triviaBefore([ BlockComment("Comment before", newlineAfter = true) ]) }
        }
        |> producesValid
            """
(*Comment before
*)
let x = 10
"""

    [<Fact>]
    let ``Produces block comment before with new line before and after``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaBefore([ BlockComment("Comment before", newlineBefore = true, newlineAfter = true) ])
            }
        }
        |> producesValid
            """

(*
Comment before
*)
let x = 10
"""

    [<Fact>]
    let ``Produces block comment before with new line before and after(NewLine)``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaBefore(
                        [ Newline()
                          BlockComment("Comment before", newlineBefore = true, newlineAfter = true)
                          Newline() ]
                    )
            }
        }
        |> producesValid
            """

(*
Comment before
*)

let x = 10
"""

    [<Fact>]
    let ``Produces block comment before with new line before and after(NewLine) and multiple comments``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaBefore(
                        [ Newline()
                          BlockComment("Comment before", newlineBefore = true, newlineAfter = true)
                          Newline()
                          BlockComment("Another comment", newlineBefore = true, newlineAfter = true)
                          Newline() ]
                    )
            }
        }
        |> producesValid
            """

(*
Comment before
*)

(*
Another comment
*)

let x = 10
"""

    // commentsAfter

    [<Fact>]
    let ``Produces line comment after``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaAfter([ SingleLine("Comment after") ]) } }
        |> producesValid
            """
let x = 10
// Comment after
"""

    [<Fact>]
    let ``Produces line comment after with multiple values``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaAfter([ SingleLine("Comment after"); SingleLine("Another comment") ])
            }
        }
        |> producesValid
            """
let x = 10
// Comment after
// Another comment
"""

    [<Fact>]
    let ``Produces line comment after with a new line before``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaAfter([ Newline(); SingleLine("Comment after") ]) } }
        |> producesValid
            """
let x = 10

// Comment after
"""

    [<Fact>]
    let ``Produces line comment after with a new line after``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaAfter([ SingleLine("Comment after"); Newline() ]) } }
        |> producesValid
            """
let x = 10
// Comment after

"""

    [<Fact>]
    let ``Produces line comment after with a new line before and after``() =
        Oak() {
            AnonymousModule() { Value("x", "10").triviaAfter([ Newline(); SingleLine("Comment after"); Newline() ]) }
        }
        |> producesValid
            """
let x = 10

// Comment after

"""

    [<Fact>]
    let ``Produces line comment after source code(commentsAfter)``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10").triviaAfter([ LineCommentAfterSourceCode("Comment after source code") ])
            }
        }
        |> producesValid
            """
let x = 10 // Comment after source code
"""

    [<Fact>]
    let ``Produces block comment after``() =
        Oak() { AnonymousModule() { Value("x", "10").triviaAfter([ BlockComment("Comment after") ]) } }
        |> producesValid
            """
let x = 10 (*Comment after*)
"""

    [<Fact>]
    let ``Produces block comment after with new line before``() =
        Oak() {
            AnonymousModule() { Value("x", "10").triviaAfter([ BlockComment("Comment after", newlineBefore = true) ]) }
        }
        |> producesValid
            """
let x = 10
(*
Comment after*)
"""

    [<Fact>]
    let ``Produces block comment after with new line after``() =
        Oak() {
            AnonymousModule() { Value("x", "10").triviaAfter([ BlockComment("Comment after", newlineAfter = true) ]) }
        }
        |> producesValid
            """
let x = 10 (*Comment after
*)
"""

    [<Fact>]
    let ``Produces block comment after with new line before and after``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaAfter([ BlockComment("Comment after", newlineBefore = true, newlineAfter = true) ])
            }
        }
        |> producesValid
            """
let x = 10
(*
Comment after
*)"""

    [<Fact>]
    let ``Produces block comment after with new line before and after(NewLine)``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaAfter(
                        [ Newline()
                          BlockComment("Comment after", newlineBefore = true, newlineAfter = true)
                          Newline() ]
                    )
            }
        }
        |> producesValid
            """
let x = 10

(*
Comment after
*)

"""

    [<Fact>]
    let ``Produces block comment after with new line before and after(NewLine) and multiple comments``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaAfter(
                        [ Newline()
                          BlockComment("Comment after", newlineBefore = true, newlineAfter = true)
                          Newline()
                          BlockComment("Another comment", newlineBefore = true, newlineAfter = true)
                          Newline() ]
                    )
            }
        }
        |> producesValid
            """
let x = 10

(*
Comment after
*)

(*
Another comment
*)
"""

    [<Fact>]
    let ``Produces block comment after with new line before and after(NewLine) and multiple comments and line comments``
        ()
        =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .triviaAfter(
                        [ Newline()
                          BlockComment("Comment after", newlineBefore = true, newlineAfter = true)
                          Newline()
                          BlockComment("Another comment", newlineBefore = true, newlineAfter = true)
                          Newline()
                          SingleLine("Line comment after") ]
                    )
            }
        }
        |> producesValid
            """
let x = 10

(*
Comment after
*)

(*
Another comment
*)

// Line comment after
"""

    [<Fact>]
    let ``Produces block comment after with new line before and after(NewLine) and multiple comments and line comments multiple modifiers``
        ()
        =
        Oak() {
            AnonymousModule() {
                Value("x", "1").triviaBefore(TriviaNode(SingleLine("Comment before")))

                Value("x", "10")
                    .triviaAfter(Newline())
                    .triviaAfter(BlockComment("Comment after", newlineBefore = true, newlineAfter = true))
                    .triviaAfter(Newline())
                    .triviaAfter(BlockComment("Another comment", newlineBefore = true, newlineAfter = true))
                    .triviaAfter(Newline())
                    .triviaAfter(SingleLine("Line comment after"))
                    .triviaBefore(Directive("#r \"nuget: Fantomas.Core.SyntaxOak\""))
            }
        }
        |> producesValid
            """
// Comment before
let x = 1
#r "nuget: Fantomas.Core.SyntaxOak"
let x = 10

(*
Comment after
*)

(*
Another comment
*)

// Line comment after
"""
