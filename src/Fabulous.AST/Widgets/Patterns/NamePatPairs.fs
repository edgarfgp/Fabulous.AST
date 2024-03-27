namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamePatPairs =
    let Pairs = Attributes.defineScalar<NamePatPair list> "Items"

    let Identifiers = Attributes.defineScalar<string> "Identifiers"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
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

            let identifier = Widgets.getScalarValue widget Identifiers

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

        static member NamePatPairsPat(ident: string, pairs: WidgetBuilder<NamePatPair> list) =
            WidgetBuilder<Pattern>(
                NamePatPairs.WidgetKey,
                NamePatPairs.Pairs.WithValue(pairs |> List.map Gen.mkOak),
                NamePatPairs.Identifiers.WithValue(ident)
            )
