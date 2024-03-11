namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameters =
    let Parameters = Attributes.defineWidgetCollection "Parameters"
    let HasTupledParameters = Attributes.defineScalar<bool> "HasTupledParameters"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let parameters = Widgets.getNodesFromWidgetCollection<Pattern> widget Parameters
            let isTupled = Widgets.getScalarValue widget HasTupledParameters

            if isTupled then
                parameters
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))
                |> (fun p -> PatTupleNode(p, Range.Zero))
                |> Pattern.Tuple
                |> (fun pNode ->
                    Pattern.Paren(
                        PatParenNode(
                            SingleTextNode.leftParenthesis,
                            pNode,
                            SingleTextNode.rightParenthesis,
                            Range.Zero
                        )
                    ))
            else
                parameters
                |> List.map(fun pNode ->
                    match pNode with
                    | Pattern.Named nd -> Pattern.Named nd
                    | pNode ->
                        Pattern.Paren(
                            PatParenNode(
                                SingleTextNode.leftParenthesis,
                                pNode,
                                SingleTextNode.rightParenthesis,
                                Range.Zero
                            )
                        ))
                |> (fun pNodes ->
                    Pattern.LongIdent(PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pNodes, Range.Zero))))

[<AutoOpen>]
module ParametersBuilders =
    type Ast with

        static member inline ParametersPat(isTupled: bool) =
            CollectionBuilder<Pattern, Pattern>(
                Parameters.WidgetKey,
                Parameters.Parameters,
                Parameters.HasTupledParameters.WithValue(isTupled)
            )

        static member inline ParametersPat() = Ast.ParametersPat(false)
