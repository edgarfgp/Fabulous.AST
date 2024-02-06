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
        let caret = SingleTextNode.Create "^"
        let divOp = SingleTextNode.Create "/"
        let ``new`` = SingleTextNode.Create "new"
        let ``lazy`` = SingleTextNode.Create "lazy"
        let ``null`` = SingleTextNode.Create "null"
        let ``struct`` = SingleTextNode.Create "struct"
        let empty = SingleTextNode.Create ""
        let ``static`` = SingleTextNode.Create "static"
        let ``override`` = SingleTextNode.Create "override"
        let leftQuotation = SingleTextNode.Create "<@"
        let rightQuotation = SingleTextNode.Create "@>"
        let leftBracket = SingleTextNode.Create "["
        let rightBracket = SingleTextNode.Create "]"
        let rightArray = SingleTextNode.Create "|]"
        let leftArray = SingleTextNode.Create "[|"
        let ``match`` = SingleTextNode.Create "match"
        let bar = SingleTextNode.Create "|"
        let leftCurlyBraceWithBar = SingleTextNode.Create "{|"
        let rightCurlyBraceWithBar = SingleTextNode.Create "|}"
        let underscore = SingleTextNode.Create "_"
        let ``as`` = SingleTextNode.Create "as"
        let doubleColon = SingleTextNode.Create "::"


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

        static member inline MeasurePower(baseMeasure: string, exponent: string) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(baseMeasure)) ], Range.Zero)
                    ),
                    RationalConstNode.Integer(SingleTextNode.Create(exponent)),
                    Range.Zero
                )
            )

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

        static member inline MeasurePower
            (
                baseMeasure: string,
                numerator: string,
                divOp: string,
                denominator: string
            ) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(baseMeasure)) ], Range.Zero)
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

        static member inline MeasurePower(baseMeasure: string, node: RationalConstNode) : Type =
            Type.MeasurePower(
                TypeMeasurePowerNode(
                    Type.LongIdent(
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(baseMeasure)) ], Range.Zero)
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

        static member inline Tuple(first: string, second: string, exponent: Type) : Type =
            Type.Tuple(
                TypeTupleNode(
                    [ Choice1Of2(
                          Type.AppPostfix(
                              TypeAppPostFixNode(
                                  Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent(first) ])),
                                  Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent(second) ])),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent("/") ])))

                      Choice1Of2(exponent) ],
                    Range.Zero
                )
            )

        static member inline Tuple(first: string, exponent: Type) : Type =
            Type.Tuple(
                TypeTupleNode(
                    [ Choice1Of2(
                          Type.AppPostfix(
                              TypeAppPostFixNode(
                                  Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent(first) ])),
                                  Type.LongIdent(IdentListNode.Create([])),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(Type.LongIdent(IdentListNode.Create([ IdentifierOrDot.CreateIdent("/") ])))

                      Choice1Of2(exponent) ],
                    Range.Zero
                )
            )

    type RationalConstNode with
        static member inline Integer(text: string) =
            RationalConstNode.Integer(SingleTextNode.Create(text))

        static member inline Rational(numerator: string, divOp: string, denominator: string) =
            RationalConstNode.Rational(
                RationalNode(
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.Create(numerator),
                    SingleTextNode.Create(divOp),
                    SingleTextNode.Create(denominator),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Negate(node: RationalConstNode) =
            RationalConstNode.Negate(NegateRationalNode(SingleTextNode.minus, node, Range.Zero))

    type Measure with

        static member inline Single(measure: string) =
            Measure.Single(SingleTextNode.Create(measure))

        static member inline Operator(lhs: Measure, rhs: Measure) =
            Measure.Operator(MeasureOperatorNode(lhs, SingleTextNode.star, rhs, Range.Zero))

        static member inline Divide(lhs: Measure option, rhs: Measure) =
            Measure.Divide(MeasureDivideNode(lhs, SingleTextNode.divOp, rhs, Range.Zero))

        static member inline Power(measure: Measure, rational: RationalConstNode) =
            Measure.Power(MeasurePowerNode(measure, SingleTextNode.caret, rational, Range.Zero))

        static member inline Multiply(lhs: Measure, rhs: Measure) =
            Measure.Operator(MeasureOperatorNode(lhs, SingleTextNode.star, rhs, Range.Zero))

        static member inline Sequence(measures: Measure list) =
            Measure.Seq(MeasureSequenceNode(measures, Range.Zero))

        static member inline Parenthesis(measure: Measure) =
            Measure.Paren(
                MeasureParenNode(SingleTextNode.leftParenthesis, measure, SingleTextNode.rightParenthesis, Range.Zero)
            )

    type Constant with
        static member inline Text(text: string) =
            Constant.FromText(SingleTextNode.Create(text))

        static member inline Unit() =
            Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))

        static member inline Measure(constant: Constant, measure: Measure) =
            Constant.Measure(
                ConstantMeasureNode(
                    constant,
                    UnitOfMeasureNode(SingleTextNode.lessThan, measure, SingleTextNode.greaterThan, Range.Zero),
                    Range.Zero
                )
            )

    type Expr with

        static member inline Lazy(expr: Expr) =
            Expr.Lazy(ExprLazyNode(SingleTextNode.``lazy``, expr, Range.Zero))

        static member inline Single(leading: string, addSpace: bool, supportsStroustrup: bool, expr: Expr) =
            Expr.Single(ExprSingleNode(SingleTextNode.Create(leading), addSpace, supportsStroustrup, expr, Range.Zero))

        static member inline Constant(constant: Constant) = Expr.Constant(constant)

        static member inline Null() = Expr.Null(SingleTextNode.``null``)

        static member inline Quote(openToken: string, expr: Expr, closeToken: string) =
            Expr.Quote(
                ExprQuoteNode(SingleTextNode.Create(openToken), expr, SingleTextNode.Create(closeToken), Range.Zero)
            )

        static member inline Typed(expr: Expr, operator: string, ``type``: Type) =
            Expr.Typed(ExprTypedNode(expr, operator, ``type``, Range.Zero))

        static member inline New(``type``: Type, arguments: Expr) =
            Expr.New(ExprNewNode(SingleTextNode.``new``, ``type``, arguments, Range.Zero))

        static member inline Tuple(expressions: Expr list) =
            Expr.Tuple(
                ExprTupleNode(
                    [ for expr in expressions do
                          Choice1Of2(expr) ],
                    Range.Zero
                )
            )

        static member inline StructTuple(expressions: Expr list) =
            Expr.StructTuple(
                ExprStructTupleNode(
                    SingleTextNode.``struct``,
                    ExprTupleNode(
                        [ for expr in expressions do
                              Choice1Of2(expr) ],
                        Range.Zero
                    ),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

    type TyparDecls with
        static member inline Postfix(decl: string) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero) ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Postfix(decls: string list) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ for v in decls do
                          TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Postfix(decl: TyparDeclNode) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(SingleTextNode.lessThan, [ decl ], [], SingleTextNode.greaterThan, Range.Zero)
            )

        static member inline Postfix(decls: TyparDeclNode list) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ for v in decls do
                          v ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Prefix(decl: string) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero) ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decls: string list) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ for v in decls do
                          TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decl: TyparDeclNode) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ decl ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decls: TyparDeclNode list) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ for v in decls do
                          v ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Single(decl: string) =
            TyparDecls.SinglePrefix(TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero))

        static member inline Single(decl: TyparDeclNode) = TyparDecls.SinglePrefix(decl)
