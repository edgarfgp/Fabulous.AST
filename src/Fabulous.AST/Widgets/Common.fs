namespace Fabulous.AST

open System
open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

type AccessControl =
    | Public
    | Private
    | Internal

[<AutoOpen>]
module Auxiliary =

    type SingleTextNode =
        static member inline Create(idText: string) = SingleTextNode(idText, Range.Zero)

    module SingleTextNode =

        let lessThan = SingleTextNode.Create "<"
        let greaterThan = SingleTextNode.Create ">"
        let equals = SingleTextNode.Create "="
        let ``type`` = SingleTextNode.Create "type"
        let leftAttribute = SingleTextNode.Create "[<"
        let rightAttribute = SingleTextNode.Create ">]"

    type IdentifierOrDot =
        static member inline CreateIdent(idText: string) =
            IdentifierOrDot.Ident(SingleTextNode.Create idText)

        static member inline CreateKnownDot(idText: string) =
            IdentifierOrDot.KnownDot(SingleTextNode.Create idText)

    type IdentListNode =
        static member inline Create(content: Fantomas.Core.SyntaxOak.IdentifierOrDot list) =
            IdentListNode(content, Range.Zero)

        static member inline Create(content: Fantomas.Core.SyntaxOak.IdentifierOrDot) = IdentListNode.Create [ content ]

    type AttributeNode =
        static member inline Create
            (
                typeName: Fantomas.Core.SyntaxOak.IdentListNode,
                ?expr: Expr,
                ?target: Fantomas.Core.SyntaxOak.SingleTextNode
            ) =
            AttributeNode(typeName, expr, target, Range.Zero)

    type AttributeListNode =
        static member inline Create(attributes: Fantomas.Core.SyntaxOak.AttributeNode list) =
            AttributeListNode(SingleTextNode.leftAttribute, attributes, SingleTextNode.rightAttribute, Range.Zero)

    type MultipleAttributeListNode =
        static member inline Create(attributeLists: Fantomas.Core.SyntaxOak.AttributeListNode list) =
            MultipleAttributeListNode(attributeLists, Range.Zero)

        static member inline Create(attributeLists: Fantomas.Core.SyntaxOak.AttributeListNode) =
            MultipleAttributeListNode.Create [ attributeLists ]

        static member inline Create(attributeLists: Fantomas.Core.SyntaxOak.AttributeNode list) =
            AttributeListNode.Create attributeLists |> MultipleAttributeListNode.Create

        static member inline Create(attributeLists: Fantomas.Core.SyntaxOak.AttributeNode) =
            AttributeListNode.Create [ attributeLists ] |> MultipleAttributeListNode.Create

        static member inline Create(idTexts: string list) =
            MultipleAttributeListNode.Create(
                [ for v in idTexts do
                      AttributeNode.Create(IdentListNode.Create(IdentifierOrDot.CreateIdent v)) ]
            )


    type Type with

        /// <summary>Create a type from a string.</summary>
        /// <exception cref="System.InvalidOperationException">
        /// Your input should be a single string text identifier.
        /// Nothing more complex.
        /// </exception>
        static member inline FromString(typeName: string) : Type =
            // TODO: consider validating the input here.
            // If something complex was passed in, it would be nice to throw an exception.
            // For now, we just assume that the input is valid.
            // Bad example would be: "int -> int", use Type.Fun instead.
            [ IdentifierOrDot.CreateIdent(typeName) ]
            |> IdentListNode.Create
            |> Type.LongIdent
