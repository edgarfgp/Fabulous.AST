namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Trivia =
    [<Fact>]
    let ``Produces trivia before and after``() =
        Oak() {
            AnonymousModule() {
                Value("x", "10")
                    .lineCommentBefore("Comment before")
                    .lineCommentBeforeSource("Comment before source code")
                    .lineCommentAfterSource("Comment after source code")
                    .lineCommentAfter("Comment after")
            }
        }
        |> produces
            """
// Comment before
let x =// Comment before source code
    10 // Comment after source code
// Comment after
"""
