namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Chain =
    let Value = Attributes.defineScalar<ChainLink list> "Value"

    let WidgetKey =
        Widgets.register "Chain" (fun widget ->
            let chains = Widgets.getScalarValue widget Value
            Expr.Chain(ExprChain(chains, Range.Zero)))

[<AutoOpen>]
module ChainBuilders =
    type Ast with

        static member ChainExpr(value: WidgetBuilder<ChainLink> list) =
            let chains = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                Chain.WidgetKey,
                AttributesBundle(StackList.one(Chain.Value.WithValue(chains)), Array.empty, Array.empty)
            )

        static member ChainExpr(value: WidgetBuilder<ChainLink>) = Ast.ChainExpr([ value ])
