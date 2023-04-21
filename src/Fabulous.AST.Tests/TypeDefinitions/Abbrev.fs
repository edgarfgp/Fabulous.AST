namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Fabulous.AST.Ast

module Abbrev =

    [<Test>]
    let ``Produces type Abbrev`` () =
        AnonymousModule() { Abbrev("MyInt", Type.FromString("int")) }

        |> produces
            """

type MyInt = int

"""

    [<Test>]
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
                Type.FromString("float"),
                [],
                Range.Zero
            )

        AnonymousModule() {
            Abbrev("MyInt", Type.FromString("int"))
            EscapeHatch(alias)
        }

        |> produces
            """

type MyInt = int
type MyFloat = float

"""

    [<Test>]
    let ``Produces type Abbrev with TypeDefnAbbrevNode`` () =
        AnonymousModule() {
            Abbrev("MyInt", Type.FromString("int"))

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
                Type.FromString("float"),
                [],
                Range.Zero
            )
        }

        |> produces
            """

type MyInt = int
type MyFloat = float

"""
