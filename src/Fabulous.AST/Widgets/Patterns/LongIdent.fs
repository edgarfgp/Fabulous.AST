namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module LongIdentPattern =
    let Pairs = Attributes.defineScalar<Pattern seq> "Items"

    let Identifiers = Attributes.defineScalar<string> "Identifiers"

    let WidgetKey =
        Widgets.register "LongIdent" (fun widget ->
            let items = Widgets.getScalarValue widget Pairs
            let typeParams = Widgets.tryGetScalarValue widget Pattern.TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let identifier =
                Widgets.tryGetScalarValue widget Identifiers
                |> ValueOption.map(fun value -> [ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ])
                |> ValueOption.defaultValue []

            let accessControl =
                Widgets.tryGetScalarValue widget Pattern.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            Pattern.LongIdent(
                PatLongIdentNode(
                    accessControl,
                    IdentListNode(identifier, Range.Zero),
                    typeParams,
                    List.ofSeq items,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module LongIdentPatternBuilders =
    type Ast with

        static member LongIdentPat(pairs: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(
                LongIdentPattern.WidgetKey,
                LongIdentPattern.Pairs.WithValue(pairs |> Seq.map Gen.mkOak)
            )

        static member LongIdentPat(pair: WidgetBuilder<Pattern>) = Ast.LongIdentPat([ pair ])

        static member LongIdentPat(pairs: WidgetBuilder<Constant> seq) =
            Ast.LongIdentPat(pairs |> Seq.map Ast.ConstantPat)

        static member LongIdentPat(pair: WidgetBuilder<Constant>) = Ast.LongIdentPat([ pair ])

        static member LongIdentPat(pairs: string seq) =
            Ast.LongIdentPat(pairs |> Seq.map Ast.Constant)

        static member LongIdentPat(pair: string) = Ast.LongIdentPat([ pair ])

        static member LongIdentPat(ident: string, pairs: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(
                LongIdentPattern.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        LongIdentPattern.Identifiers.WithValue(ident),
                        LongIdentPattern.Pairs.WithValue(pairs |> Seq.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member LongIdentPat(ident: string, pair: WidgetBuilder<Pattern>) = Ast.LongIdentPat(ident, [ pair ])

        static member LongIdentPat(ident: string, pairs: WidgetBuilder<Constant> seq) =
            Ast.LongIdentPat(ident, pairs |> Seq.map Ast.ConstantPat)

        static member LongIdentPat(ident: string, pair: WidgetBuilder<Constant>) = Ast.LongIdentPat(ident, [ pair ])

        static member LongIdentPat(ident: string, pairs: string seq) =
            Ast.LongIdentPat(ident, pairs |> Seq.map Ast.Constant)

        static member LongIdentPat(ident: string, pair: string) = Ast.LongIdentPat(ident, [ pair ])
