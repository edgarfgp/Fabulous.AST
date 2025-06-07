namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NamePatPairs =
    let Pairs = Attributes.defineScalar<NamePatPair seq> "Items"

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
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let identifier =
                Widgets.getScalarValue widget Identifiers
                |> PrettyNaming.NormalizeIdentifierBackticks

            Pattern.NamePatPairs(
                PatNamePatPairsNode(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero),
                    typeParams,
                    SingleTextNode.leftParenthesis,
                    items,
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module NamePatPairsBuilders =
    type Ast with

        static member NamePatPairsPat(ident: string, pairs: WidgetBuilder<NamePatPair> seq) =
            WidgetBuilder<Pattern>(
                NamePatPairs.WidgetKey,
                NamePatPairs.Pairs.WithValue(pairs |> Seq.map Gen.mkOak),
                NamePatPairs.Identifiers.WithValue(ident)
            )
