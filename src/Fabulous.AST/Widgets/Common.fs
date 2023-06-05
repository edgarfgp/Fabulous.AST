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
    type Type with

        /// <summary>Create a type from a string.</summary>
        /// <exception cref="System.InvalidOperationException">
        /// Your input should be a single string text identifier.
        /// Nothing more complex.
        /// </exception>
        static member FromString(typeName: string) : Type =
            // TODO: consider validating the input here.
            // If something complex was passed in, it would be nice to throw an exception.
            // For now, we just assume that the input is valid.
            // Bad example would be: "int -> int", use Type.Fun instead.
            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(typeName, Range.Zero)) ], Range.Zero)
            |> Type.LongIdent

    type SingleTextNode =
        static member inline Create(idText: string) = SingleTextNode(idText, Range.Zero)

    module SingleTextNode =

        let lessThan = SingleTextNode.Create "<"
        let greaterThan = SingleTextNode.Create ">"
        let equals = SingleTextNode.Create "="
        let ``type`` = SingleTextNode.Create "type"
        let leftAttribute = SingleTextNode.Create "[<"
        let rightAttribute = SingleTextNode.Create ">]"

    type MultipleAttributeListNode =
        static member inline Create(values: string list) =
            MultipleAttributeListNode(
                [ AttributeListNode(
                      SingleTextNode.leftAttribute,
                      [ for v in values do
                            AttributeNode(
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create v) ], Range.Zero),
                                None,
                                None,
                                Range.Zero
                            ) ],
                      SingleTextNode.rightAttribute,
                      Range.Zero
                  ) ],
                Range.Zero
            )
