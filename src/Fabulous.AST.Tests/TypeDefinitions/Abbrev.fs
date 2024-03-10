namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Fabulous.AST.Ast

module Abbrev =

    [<Fact>]
    let ``Produces type Abbrev`` () =
        AnonymousModule() { Abbrev("MyInt", Int32()) }

        |> produces
            """

type MyInt = int

"""

    [<Fact>]
    let ``Produces type Abbrev using an escape hatch`` () =
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

        AnonymousModule() {
            Abbrev("MyInt", Int32())
            EscapeHatch(alias)
        }

        |> produces
            """

type MyInt = int
type MyFloat = float

"""

    [<Fact>]
    let ``Produces type Abbrev with TypeDefnAbbrevNode`` () =
        AnonymousModule() {
            Abbrev("MyInt", Int32())

            Abbrev("MyString", "string")

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
        }

        |> produces
            """

type MyInt = int
type MyString = string
type MyFloat = float

"""
