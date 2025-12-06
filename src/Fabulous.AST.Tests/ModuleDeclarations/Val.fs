namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module ValTests =

    [<Fact>]
    let ``Produces a Val``() =
        Oak() { AnonymousModule() { Val("x", LongIdent("string")) } }
        |> produces
            """
val x: string
"""

    [<Fact>]
    let ``Produces a MutableVal``() =
        Oak() { AnonymousModule() { Val("x", String()).toMutable() } }

        |> produces
            """
val mutable x: string
"""

    [<Fact>]
    let ``Produces a InlinedVal``() =
        Oak() { AnonymousModule() { Val("x", LongIdent("string")).toInlined() } }

        |> produces
            """
val inline x: string
"""

    [<Fact>]
    let ``Produces a Val with attribute``() =
        Oak() { AnonymousModule() { Val("x", String()).attribute(Attribute "DefaultValue") } }

        |> produces
            """
[<DefaultValue>]
val x: string
"""

    [<Fact>]
    let ``Produces a Val with attributes``() =
        Oak() {
            AnonymousModule() {
                Val("x", String())
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
                Val("x", String()).attribute(Attribute "DefaultValue").toInternal()

                Val("y", String()).attribute(Attribute "DefaultValue").toPrivate()

                Val("z", String()).attribute(Attribute "DefaultValue").toPublic()
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
        Oak() { AnonymousModule() { Val("x", String()).typeParams(PostfixList([ "'a"; "'b" ])) } }

        |> produces
            """
val x<'a, 'b> : string
"""

    [<Fact>]
    let ``yield! multiple vals``() =
        Oak() { AnonymousModule() { yield! [ Val("x", String()); Val("y", Int()); Val("z", Boolean()) ] } }
        |> produces
            """
val x: string
val y: int
val z: bool
"""
