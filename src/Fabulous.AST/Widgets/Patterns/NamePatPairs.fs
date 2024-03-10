namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamePatPairs =
    let Pairs = Attributes.defineWidgetCollection "Items"

    let Identifiers = Attributes.defineScalar<string> "Identifiers"

    let TypeParams = Attributes.defineScalar<string list> "TyparDecls"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
            let items = Helpers.getNodesFromWidgetCollection<NamePatPair> widget Pairs
            let typeParams = Helpers.tryGetScalarValue widget TypeParams

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

            let identifier = Helpers.getScalarValue widget Identifiers

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

        static member NamePatPairsPat(ident: string) =
            CollectionBuilder<Pattern, NamePatPair>(
                NamePatPairs.WidgetKey,
                NamePatPairs.Pairs,
                NamePatPairs.Identifiers.WithValue(ident)
            )

        static member NamePatPairsPat(ident: string, typeParams: string list) =
            CollectionBuilder<Pattern, NamePatPair>(
                NamePatPairs.WidgetKey,
                NamePatPairs.Pairs,
                NamePatPairs.Identifiers.WithValue(ident),
                NamePatPairs.TypeParams.WithValue(typeParams)
            )

[<Extension>]
type NamePatPairsYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, NamePatPair>, x: WidgetBuilder<NamePatPair>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
