namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module ValTests =

    [<Test>]
    let ``Produces a Val`` () =
        AnonymousModule() { Val("x", "string") }

        |> produces
            """
val x: string
"""

    [<Test>]
    let ``Produces a MutableVal`` () =
        AnonymousModule() { MutableVal("x", CommonType.String) }

        |> produces
            """
val mutable x: string
"""

    [<Test>]
    let ``Produces a InlinedVal`` () =
        AnonymousModule() { InlinedVal("x", "string") }

        |> produces
            """
val inline x: string
"""

    [<Test>]
    let ``Produces a Val with attribute`` () =
        AnonymousModule() {
            Val("x", CommonType.String)
                .attributes(AttributeNode("DefaultValue"))
        }

        |> produces
            """
[<DefaultValue>]
val x: string
"""

    [<Test>]
    let ``Produces a Val with accessControl`` () =
        AnonymousModule() {
            Val("x", CommonType.String)
                .attributes(AttributeNode("DefaultValue"))
                .toInternal()

            Val("y", CommonType.String)
                .attributes(AttributeNode("DefaultValue"))
                .toPrivate()

            Val("z", CommonType.String)
                .attributes(AttributeNode("DefaultValue"))
                .toPublic()
        }

        |> produces
            """
[<DefaultValue>]
val internal x: string

[<DefaultValue>]
val private y: string

[<DefaultValue>]
val public z: string
"""

    [<Test>]
    let ``Produces a Val with type parameters`` () =
        AnonymousModule() { Val("x", CommonType.String).typeParameters([ "a"; "b" ]) }

        |> produces
            """
val x<a, b> : string
"""
