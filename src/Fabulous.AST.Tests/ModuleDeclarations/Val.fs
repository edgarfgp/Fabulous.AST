namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ValTests =

    [<Fact>]
    let ``Produces a Val``() =
        Oak() { AnonymousModule() { ValField("x", LongIdent("string")) } }
        |> produces
            """
val x: string
"""

    [<Fact>]
    let ``Produces a MutableVal``() =
        Oak() { AnonymousModule() { ValField("x", String()).toMutable() } }

        |> produces
            """
val mutable x: string
"""

    [<Fact>]
    let ``Produces a InlinedVal``() =
        Oak() { AnonymousModule() { ValField("x", LongIdent("string")).toInlined() } }

        |> produces
            """
val inline x: string
"""

    [<Fact>]
    let ``Produces a Val with attribute``() =
        Oak() { AnonymousModule() { ValField("x", String()).attribute(Attribute "DefaultValue") } }

        |> produces
            """
[<DefaultValue>]
val x: string
"""

    [<Fact>]
    let ``Produces a Val with attributes``() =
        Oak() {
            AnonymousModule() {
                ValField("x", String())
                    .attributes(
                        [ Attribute "DefaultValue"
                          Attribute "OtherAttribute"
                          Attribute "AnotherAttribute" ]
                    )
            }
        }

        |> produces
            """
[<DefaultValue; OtherAttribute; AnotherAttribute>]
val x: string
"""

    [<Fact>]
    let ``Produces a Val with accessControl``() =
        Oak() {
            AnonymousModule() {
                ValField("x", String()).attribute(Attribute "DefaultValue").toInternal()

                ValField("y", String()).attribute(Attribute "DefaultValue").toPrivate()

                ValField("z", String()).attribute(Attribute "DefaultValue").toPublic()
            }
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
        Oak() { AnonymousModule() { ValField("x", String()).typeParams(PostfixList([ "'a"; "'b" ])) } }

        |> produces
            """
val x<'a, 'b> : string
"""
