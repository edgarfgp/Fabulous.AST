namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type AccessControl =
    | Public
    | Private
    | Internal
    | Unknown

[<AutoOpen>]
module Auxiliary =
    type SingleTextNode =
        static member inline Create(idText: string) = SingleTextNode(idText, Range.Zero)

    module SingleTextNode =

        let lessThan = SingleTextNode.Create "<"
        let greaterThan = SingleTextNode.Create ">"
        let equals = SingleTextNode.Create "="
        let comma = SingleTextNode.Create ","
        let ``type`` = SingleTextNode.Create "type"
        let leftAttribute = SingleTextNode.Create "[<"
        let rightAttribute = SingleTextNode.Create ">]"
        let leftCurlyBrace = SingleTextNode.Create "{"
        let rightCurlyBrace = SingleTextNode.Create "}"
        let leftParenthesis = SingleTextNode.Create "("
        let rightParenthesis = SingleTextNode.Create ")"
        let ``abstract`` = SingleTextNode.Create "abstract"
        let ``interface`` = SingleTextNode.Create "interface"
        let ``class`` = SingleTextNode.Create "class"
        let ``end`` = SingleTextNode.Create "end"
        let ``with`` = SingleTextNode.Create "with"
        let ``member`` = SingleTextNode.Create "member"
        let rightArrow = SingleTextNode.Create "->"
        let lefArrow = SingleTextNode.Create "<-"
        let star = SingleTextNode.Create "*"
        let ``if`` = SingleTextNode.Create "if"
        let ``elif`` = SingleTextNode.Create "elif"
        let ``then`` = SingleTextNode.Create "then"
        let ``else`` = SingleTextNode.Create "else"
        let ``let`` = SingleTextNode.Create "let"
        let ``public`` = SingleTextNode.Create "public"
        let ``private`` = SingleTextNode.Create "private"
        let ``internal`` = SingleTextNode.Create "internal"
        let ``inline`` = SingleTextNode.Create "inline"
        let ``namespace`` = SingleTextNode.Create "namespace"
        let ``module`` = SingleTextNode.Create "module"
        let colon = SingleTextNode.Create ":"
        let minus = SingleTextNode.Create "-"

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

    type MultipleAttributeListNode with

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

    type TypeSignatureParameterNode with

        static member Create(identifier: string, ``type``: Type) =
            TypeSignatureParameterNode(None, Some(SingleTextNode.Create(identifier)), ``type``, Range.Zero)

    type MultipleTextsNode with

        static member Create(texts: string list) =
            MultipleTextsNode(
                [ for v in texts do
                      SingleTextNode.Create(v) ],
                Range.Zero
            )

    type MemberDefnAbstractSlotNode with

        static member Method(identifier: string, parameters: Type) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                parameters,
                None,
                Range.Zero
            )

        static member Property(identifier: string, returnType: Type) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                None,
                Range.Zero
            )

        static member GetSet(identifier, returnType) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ])),
                Range.Zero
            )

        static member Get(identifier, returnType) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                Some(MultipleTextsNode.Create([ "with"; "get" ])),
                Range.Zero
            )

        static member Set(identifier, returnType) =
            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                Type.Funs(TypeFunsNode([], returnType, Range.Zero)),
                Some(MultipleTextsNode.Create([ "with"; "set" ])),
                Range.Zero
            )

    type XmlDocNode with

        static member Create(content: string list) =
            content
            |> List.map(fun v -> $"/// {v}")
            |> Array.ofList
            |> fun v -> XmlDocNode(v, Range.Zero)

    type Type with
        static member inline MeasurePower(baseMeasure: string list, exponent: string) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode(
                            [ for ident in baseMeasure do
                                  IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ],
                            Range.Zero
                        )
                    ),
                    RationalConstNode.Integer(SingleTextNode.Create(exponent)),
                    Range.Zero
                )
            )

        static member inline MeasurePower
            (
                baseMeasure: string list,
                numerator: string,
                divOp: string,
                denominator: string
            ) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode(
                            [ for ident in baseMeasure do
                                  IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ],
                            Range.Zero
                        )
                    ),
                    RationalConstNode.Rational(
                        RationalNode(
                            SingleTextNode.leftParenthesis,
                            SingleTextNode.Create(numerator),
                            SingleTextNode.Create(divOp),
                            SingleTextNode.Create(denominator),
                            SingleTextNode.rightParenthesis,
                            Range.Zero
                        )
                    ),
                    Range.Zero
                )
            )

        static member inline MeasurePower(baseMeasure: string list, node: RationalConstNode) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode(
                            [ for ident in baseMeasure do
                                  IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ],
                            Range.Zero
                        )
                    ),
                    RationalConstNode.Negate(NegateRationalNode(SingleTextNode.minus, node, Range.Zero)),
                    Range.Zero
                )
            )

        static member inline Tuple(first: string list, second: string list, exponent: Type) : Type =
            Type.Tuple(
                TypeTupleNode(
                    [ Choice1Of2(
                          Type.AppPostfix(
                              TypeAppPostFixNode(
                                  Type.LongIdent(
                                      IdentListNode.Create(
                                          [ for ident in first do
                                                IdentifierOrDot.CreateIdent(ident) ]
                                      )
                                  ),
                                  Type.LongIdent(
                                      IdentListNode.Create(
                                          [ for ident in second do
                                                IdentifierOrDot.CreateIdent(ident) ]
                                      )
                                  ),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent("/") ])))

                      Choice1Of2(exponent) ],
                    Range.Zero
                )
            )
