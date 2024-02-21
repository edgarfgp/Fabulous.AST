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
        let isInstance = SingleTextNode.Create ":?"
        let forwardSlash = SingleTextNode.Create "/"
        let backSlash = SingleTextNode.Create "\""
        let ``mutable`` = SingleTextNode.Create "mutable"
        let ``val`` = SingleTextNode.Create "val"
        let measure = SingleTextNode.Create "Measure"
        let ``extern`` = SingleTextNode.Create "extern"

    type MultipleTextsNode with

        static member Create(texts: string list) =
            MultipleTextsNode(
                [ for v in texts do
                      SingleTextNode.Create(v) ],
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
                                      IdentListNode(
                                          [ for ident in first do
                                                IdentifierOrDot.Ident(SingleTextNode.Create ident) ],
                                          Range.Zero
                                      )
                                  ),
                                  Type.LongIdent(
                                      IdentListNode(
                                          [ for ident in second do
                                                IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ],
                                          Range.Zero
                                      )
                                  ),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(
                          Type.LongIdent(
                              IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.forwardSlash) ], Range.Zero)
                          )
                      )

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
                                  Type.LongIdent(
                                      IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(first)) ], Range.Zero)
                                  ),
                                  Type.LongIdent(
                                      IdentListNode(
                                          [ IdentifierOrDot.Ident(SingleTextNode.Create(second)) ],
                                          Range.Zero
                                      )
                                  ),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(
                          Type.LongIdent(
                              IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.forwardSlash) ], Range.Zero)
                          )
                      )

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
                                  Type.LongIdent(
                                      IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(first)) ], Range.Zero)
                                  ),
                                  Type.LongIdent(IdentListNode([], Range.Zero)),
                                  Range.Zero
                              )
                          )
                      )
                      Choice1Of2(
                          Type.LongIdent(
                              IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.forwardSlash) ], Range.Zero)
                          )
                      )

                      Choice1Of2(exponent) ],
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
