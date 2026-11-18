namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Chain =
    let Value = Attributes.defineScalar<ChainLink seq> "Value"

    let private paren(argument: Expr) =
        ChainCall.Paren(
            ExprParenNode(SingleTextNode.leftParenthesis, argument, SingleTextNode.rightParenthesis, Range.range0)
        )

    let private unitCall() =
        ChainCall.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.range0))

    let private invalidChain(reason: string) =
        invalidArg
            "links"
            $"Invalid chain: {reason} A chain is a receiver followed by `ChainLinkDot()` + one link per step, e.g. [ ChainLinkIdentifier \"A\"; ChainLinkDot(); ChainLinkAppUnit \"B\" ]."

    /// Convert source-order links into Fantomas 8's chain: a dot-free head, one segment per
    /// `.step`, and the final call (if any) as the terminal. Mirrors Fantomas's own
    /// `mkChainFromLinks`, so generated chains print exactly like parsed ones.
    let toExpr(links: ChainLink list) : Expr =
        let head, rest =
            match links with
            | ChainLink.Identifier e :: rest
            | ChainLink.Expr e :: rest -> e, rest
            // A call as the receiver (`X().Y`) is a zero-segment chain whose call stays tight:
            // `X ().Y` would parse differently.
            | ChainLink.AppUnit f :: rest ->
                Expr.Chain(ExprChain(f, [], ChainTerminal.NoSpaceAllowed(unitCall()), Range.range0)), rest
            | ChainLink.AppParen(f, a) :: rest ->
                Expr.Chain(ExprChain(f, [], ChainTerminal.NoSpaceAllowed(paren a), Range.range0)), rest
            | [] -> invalidChain "it has no links."
            | ChainLink.Dot :: _ -> invalidChain "it starts with a dot instead of a receiver."
            | ChainLink.IndexExpr _ :: _ -> invalidChain "it starts with an index instead of a receiver."

        // Links after the head come in pairs: a dot, then one step. The last pair decides the
        // terminal: only the final call of a chain may take a space before its `(`.
        let rec segments(links: ChainLink list) : ChainSegment list * ChainTerminal =
            match links with
            | [ ChainLink.Dot; (ChainLink.Identifier f | ChainLink.Expr f) ] ->
                [ ChainSegment.DotMember(SingleTextNode.dot, f) ], ChainTerminal.NoTerminal
            | [ ChainLink.Dot; ChainLink.AppUnit f ] ->
                [ ChainSegment.DotMember(SingleTextNode.dot, f) ], ChainTerminal.SpaceAllowed(unitCall())
            | [ ChainLink.Dot; ChainLink.AppParen(f, a) ] ->
                [ ChainSegment.DotMember(SingleTextNode.dot, f) ], ChainTerminal.SpaceAllowed(paren a)
            | [ ChainLink.Dot; ChainLink.IndexExpr i ] ->
                [ ChainSegment.DotIndex(SingleTextNode.dot, i) ], ChainTerminal.NoTerminal
            | ChainLink.Dot :: step :: rest ->
                let segment =
                    match step with
                    | ChainLink.Identifier f
                    | ChainLink.Expr f -> ChainSegment.DotMember(SingleTextNode.dot, f)
                    | ChainLink.AppUnit f -> ChainSegment.DotApplication(SingleTextNode.dot, f, unitCall())
                    | ChainLink.AppParen(f, a) -> ChainSegment.DotApplication(SingleTextNode.dot, f, paren a)
                    | ChainLink.IndexExpr i -> ChainSegment.DotIndex(SingleTextNode.dot, i)
                    | ChainLink.Dot -> invalidChain "it has two dots in a row."

                let rest, terminal = segments rest
                segment :: rest, terminal
            | _ -> invalidChain "every step after the receiver must be `ChainLinkDot()` followed by one link."

        match rest with
        | [] -> head
        | _ ->
            let segments, terminal = segments rest
            Expr.Chain(ExprChain(head, segments, terminal, Range.range0))

    let WidgetKey =
        Widgets.register "Chain" (fun widget -> Widgets.getScalarValue widget Value |> List.ofSeq |> toExpr)

[<AutoOpen>]
module ChainBuilders =
    type Ast with

        static member ChainExpr(value: WidgetBuilder<ChainLink> seq) =
            let chains = value |> Seq.map Gen.mkOak |> Seq.toArray

            WidgetBuilder<Expr>(Chain.WidgetKey, Chain.Value.WithValue(chains))

        static member ChainExpr(value: WidgetBuilder<ChainLink>) = Ast.ChainExpr([ value ])
