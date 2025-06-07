namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Chain =
    let Value = Attributes.defineScalar<ChainLink seq> "Value"

    let WidgetKey =
        Widgets.register "Chain" (fun widget ->
            let chains = Widgets.getScalarValue widget Value |> List.ofSeq
            Expr.Chain(ExprChain(chains, Range.Zero)))

[<AutoOpen>]
module ChainBuilders =
    type Ast with

        static member ChainExpr(value: WidgetBuilder<ChainLink> seq) =
            let chains = value |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(Chain.WidgetKey, Chain.Value.WithValue(chains))

        static member ChainExpr(value: WidgetBuilder<ChainLink>) = Ast.ChainExpr([ value ])
