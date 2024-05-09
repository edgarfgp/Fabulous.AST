namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module LongIdentPattern =
    let Pairs = Attributes.defineScalar<Pattern list> "Items"

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

            let identifier = Widgets.tryGetScalarValue widget Identifiers

            let identifier =
                match identifier with
                | ValueSome value ->
                    [ let value = PrettyNaming.NormalizeIdentifierBackticks value
                      IdentifierOrDot.Ident(SingleTextNode.Create(value)) ]
                | ValueNone -> []

            Pattern.LongIdent(
                PatLongIdentNode(None, IdentListNode(identifier, Range.Zero), typeParams, items, Range.Zero)
            ))

[<AutoOpen>]
module LongIdentPatternBuilders =
    type Ast with

        static member LongIdentPat(pairs: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                LongIdentPattern.WidgetKey,
                AttributesBundle(
                    StackList.one(LongIdentPattern.Pairs.WithValue(pairs |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member LongIdentPat(pairs: WidgetBuilder<Constant> list) =
            Ast.LongIdentPat(pairs |> List.map Ast.ConstantPat)

        static member LongIdentPat(pairs: string list) =
            Ast.LongIdentPat(pairs |> List.map Ast.Constant)

        static member LongIdentPat(ident: string, pairs: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                LongIdentPattern.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        LongIdentPattern.Identifiers.WithValue(ident),
                        LongIdentPattern.Pairs.WithValue(pairs |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member LongIdentPat(ident: string, pairs: WidgetBuilder<Constant> list) =
            Ast.LongIdentPat(ident, pairs |> List.map Ast.ConstantPat)

        static member LongIdentPat(ident: string, pairs: string list) =
            Ast.LongIdentPat(ident, pairs |> List.map Ast.Constant)
