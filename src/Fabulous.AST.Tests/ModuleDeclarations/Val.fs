namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ValTests =

    [<Fact>]
    let ``Produces a Val``() =
        AnonymousModule() { Val("x", "string") }

        |> produces
            """
val x: string
"""

    [<Fact>]
    let ``Produces a MutableVal``() =
        AnonymousModule() { Val("x", String()).toMutable() }

        |> produces
            """
val mutable x: string
"""

    [<Fact>]
    let ``Produces a InlinedVal``() =
        AnonymousModule() { Val("x", "string").toInlined() }

        |> produces
            """
val inline x: string
"""

    [<Fact>]
    let ``Produces a Val with attribute``() =
        AnonymousModule() { Val("x", String()).attribute("DefaultValue") }

        |> produces
            """
[<DefaultValue>]
val x: string
"""

    [<Fact>]
    let ``Produces a Val with attributes``() =
        AnonymousModule() {
            Val("x", String())
                .attributes([ "DefaultValue"; "OtherAttribute"; "AnotherAttribute" ])
        }

        |> produces
            """
[<DefaultValue; OtherAttribute; AnotherAttribute>]
val x: string
"""

    [<Fact>]
    let ``Produces a Val with accessControl``() =
        AnonymousModule() {
            Val("x", String()).attribute("DefaultValue").toInternal()

            Val("y", String()).attribute("DefaultValue").toPrivate()

            Val("z", String()).attribute("DefaultValue").toPublic()
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

    [<Fact>]
    let ``Produces a Val with type parameters``() =
        AnonymousModule() { Val("x", String()).typeParameters([ "a"; "b" ]) }

        |> produces
            """
val x<a, b> : string
"""
