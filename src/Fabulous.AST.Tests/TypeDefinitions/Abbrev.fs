namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Fabulous.AST.Ast

module Abbrev =

    [<Fact>]
    let ``Produces type Abbrev``() =
        Oak() { AnonymousModule() { Abbrev("MyInt", Int()) } }

        |> producesValid
            """

type MyInt = int

"""

    [<Fact>]
    let ``Produces multiple type Abbrev``() =
        Oak() {
            AnonymousModule() {
                Abbrev("SizeType", UInt32())
                Abbrev("Transform", Funs("'a", "'a")).typeParams(PostfixList("'a"))
            }
        }

        |> producesValid
            """

type SizeType = uint32
type Transform<'a> = 'a -> 'a

"""

    [<Fact>]
    let ``Produces type Abbrev with an Obsolete attribute``() =
        Oak() {
            AnonymousModule() {
                Open("System")

                Abbrev("MyInt", Int())
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(String("This is obsolete")))))
            }
        }

        |> producesValid
            """
open System

[<Obsolete("This is obsolete")>]
type MyInt = int

    """

    [<Fact>]
    let ``Produces type Abbrev with xml comments``() =
        Oak() { AnonymousModule() { Abbrev("MyInt", Int()).xmlDocs([ "hello world" ]) } }
        |> producesValid
            """
/// hello world
type MyInt = int

"""

    [<Fact>]
    let ``Produces type Abbrev with multiple xml comments``() =
        Oak() { AnonymousModule() { Abbrev("MyInt", Int()).xmlDocs([ "First comment"; "Second comment" ]) } }
        |> producesValid
            """
/// First comment
/// Second comment
type MyInt = int

    """

    [<Fact>]
    let ``Produces type Abbrev using an escape hatch``() =
        let alias =
            TypeDefnAbbrevNode(
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode("type", Range.Zero),
                    Some(SingleTextNode("MyFloat", Range.Zero)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create("float")) ], Range.Zero)),
                [],
                Range.Zero
            )

        Oak() {
            AnonymousModule() {
                Abbrev("MyInt", Int())
                EscapeHatch(TypeDefn.Abbrev(alias))
            }
        }

        |> producesValid
            """

type MyInt = int
type MyFloat = float

"""

    [<Fact>]
    let ``Produces type Abbrev with TypeDefnAbbrevNode``() =
        Oak() {
            AnonymousModule() {
                Abbrev("MyInt", Int())

                Abbrev("MyString", LongIdent "string")

                EscapeHatch(
                    TypeDefn.Abbrev(
                        TypeDefnAbbrevNode(
                            TypeNameNode(
                                None,
                                None,
                                SingleTextNode("type", Range.Zero),
                                Some(SingleTextNode("MyFloat", Range.Zero)),
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
                                None,
                                [],
                                None,
                                None,
                                None,
                                Range.Zero
                            ),
                            Type.LongIdent(
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create("float")) ], Range.Zero)
                            ),
                            [],
                            Range.Zero
                        )
                    )
                )

            }
        }

        |> producesValid
            """

type MyInt = int
type MyString = string
type MyFloat = float

"""

    [<Fact>]
    let ``Recursive abbreviation renders with `and` keyword``() =
        Oak() {
            AnonymousModule() {
                Union("Tree") { UnionCase("Leaf") }

                Abbrev("Forest", AppPrefix(LongIdent "list", [ LongIdent "Tree" ]))
                |> _.toRecursive()
            }
        }
        |> producesValid
            """
type Tree = | Leaf
and Forest = list<Tree>
"""

    [<Fact>]
    let ``yield! multiple type abbreviations``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ Abbrev("MyInt", Int())
                      Abbrev("MyString", String())
                      Abbrev("MyFloat", Float()) ]
            }
        }
        |> producesValid
            """
type MyInt = int
type MyString = string
type MyFloat = float
"""
