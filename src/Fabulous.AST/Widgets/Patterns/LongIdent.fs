namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module LongIdentPattern =
    let Pairs = Attributes.defineWidgetCollection "Items"

    let Identifiers = Attributes.defineScalar<string> "Identifiers"

    let TypeParams = Attributes.defineScalar<string list> "TyparDecls"

    let WidgetKey =
        Widgets.register "LongIdent" (fun widget ->
            let items = Widgets.getNodesFromWidgetCollection<Pattern> widget Pairs
            let typeParams = Widgets.tryGetScalarValue widget TypeParams

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

            let identifier = Widgets.tryGetScalarValue widget Identifiers

            let identifier =
                match identifier with
                | ValueSome value -> [ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ]
                | ValueNone -> []

            Pattern.LongIdent(
                PatLongIdentNode(None, IdentListNode(identifier, Range.Zero), typeParams, items, Range.Zero)
            ))

[<AutoOpen>]
module LongIdentPatternBuilders =
    type Ast with

        static member LongIdentPat() =
            CollectionBuilder<Pattern, Pattern>(LongIdentPattern.WidgetKey, LongIdentPattern.Pairs)

        static member LongIdentPat(ident: string) =
            CollectionBuilder<Pattern, Pattern>(
                LongIdentPattern.WidgetKey,
                LongIdentPattern.Pairs,
                LongIdentPattern.Identifiers.WithValue(ident)
            )

        static member LongIdentPat(ident: string, typeParams: string list) =
            CollectionBuilder<Pattern, Pattern>(
                LongIdentPattern.WidgetKey,
                LongIdentPattern.Pairs,
                LongIdentPattern.Identifiers.WithValue(ident),
                LongIdentPattern.TypeParams.WithValue(typeParams)
            )
