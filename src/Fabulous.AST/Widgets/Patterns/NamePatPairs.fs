namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NamePatPairs =
    let Pairs = Attributes.defineScalar<NamePatPairNode seq> "Items"

    let Identifiers = Attributes.defineScalar<string> "Identifiers"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
            let items = Widgets.getScalarValue widget Pairs |> List.ofSeq
            let typeParams = Widgets.tryGetScalarValue widget Pattern.TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.range0) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.range0
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let identifier =
                Widgets.getScalarValue widget Identifiers
                |> PrettyNaming.NormalizeIdentifierBackticks

            Pattern.NamePatPairs(
                PatNamePatPairsNode(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.range0),
                    typeParams,
                    SingleTextNode.leftParenthesis,
                    items,
                    SingleTextNode.rightParenthesis,
                    Range.range0
                )
            ))

[<AutoOpen>]
module NamePatPairsBuilders =
    type Ast with

        static member NamePatPairsPat(ident: string, pairs: WidgetBuilder<NamePatPairNode> seq) =
            WidgetBuilder<Pattern>(
                NamePatPairs.WidgetKey,
                NamePatPairs.Pairs.WithValue(pairs |> Seq.map Gen.mkOak |> Seq.toArray),
                NamePatPairs.Identifiers.WithValue(ident)
            )
