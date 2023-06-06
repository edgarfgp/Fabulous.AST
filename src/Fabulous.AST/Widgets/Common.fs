namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

type AccessControl =
    | Public
    | Private
    | Internal

[<AutoOpen>]
module Auxiliary =
    type BindingNode with

        static member Create(name: string, value: string, ?t: Type) =
            match t with
            | None ->
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
                    Range.Zero
                )
            | Some t ->
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                    None,
                    List.Empty,
                    Some(BindingReturnInfoNode(SingleTextNode(":", Range.Zero), t, Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
                    Range.Zero
                )

    type MemberDefnAbstractSlotNode with

        static member Method(identifier: string, parameters: (Type * SingleTextNode) list, returnType: Type) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode(
                    [ SingleTextNode("abstract", Range.Zero); SingleTextNode("member", Range.Zero) ],
                    Range.Zero
                ),
                SingleTextNode(identifier, Range.Zero),
                None,
                Type.Funs(TypeFunsNode(parameters, returnType, Range.Zero)),
                None,
                Range.Zero
            )

        static member Property(identifier: string, returnType: Type) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode(
                    [ SingleTextNode("abstract", Range.Zero); SingleTextNode("member", Range.Zero) ],
                    Range.Zero
                ),
                SingleTextNode(identifier, Range.Zero),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                None,
                Range.Zero
            )

        static member GetSet(identifier: string, returnType: Type) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode(
                    [ SingleTextNode("abstract", Range.Zero); SingleTextNode("member", Range.Zero) ],
                    Range.Zero
                ),
                SingleTextNode(identifier, Range.Zero),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                Some(
                    MultipleTextsNode(
                        [ SingleTextNode("with", Range.Zero)
                          SingleTextNode("get,", Range.Zero)
                          SingleTextNode("set", Range.Zero) ],
                        Range.Zero
                    )
                ),
                Range.Zero
            )

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

module internal TypeHelpers =
    let inline createAttributes values =
        MultipleAttributeListNode(
            [ AttributeListNode(
                  SingleTextNode("[<", Range.Zero),
                  [ for v in values do
                        AttributeNode(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(v, Range.Zero)) ], Range.Zero),
                            None,
                            None,
                            Range.Zero
                        ) ],
                  SingleTextNode(">]", Range.Zero),
                  Range.Zero
              ) ],
            Range.Zero
        )
