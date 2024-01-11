namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

module Parameter =

    let Parameters = Attributes.defineScalar<(string * Type option) list> "Parameters"
    let HasTupledParameters = Attributes.defineScalar<bool> "HasTupledParameters"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->

            let parameters = Helpers.getScalarValue widget Parameters
            let hasTupledParameters = Helpers.getScalarValue widget HasTupledParameters

            match parameters with
            | [] ->
                Pattern.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            | parameters ->
                let singleParamNodes =
                    parameters
                    |> List.map(fun (pName, pType) ->
                        match pType with
                        | None -> Pattern.Named(PatNamedNode(None, SingleTextNode(pName, Range.Zero), Range.Zero))
                        | Some _ ->
                            Pattern.Parameter(
                                PatParameterNode(
                                    None,
                                    Pattern.Named(PatNamedNode(None, SingleTextNode(pName, Range.Zero), Range.Zero)),
                                    pType,
                                    Range.Zero
                                )
                            )

                    )

                match hasTupledParameters with
                | true ->
                    singleParamNodes
                    |> List.map Choice1Of2
                    |> List.intersperse(Choice2Of2(SingleTextNode(",", Range.Zero)))
                    |> (fun p -> PatTupleNode(p, Range.Zero))
                    |> Pattern.Tuple
                    |> (fun pNode ->
                        Pattern.Paren(
                            PatParenNode(
                                SingleTextNode("(", Range.Zero),
                                pNode,
                                SingleTextNode(")", Range.Zero),
                                Range.Zero
                            )
                        ))

                //curried params
                | false ->
                    singleParamNodes
                    |> List.map(fun pNode ->
                        Pattern.Paren(
                            PatParenNode(
                                SingleTextNode("(", Range.Zero),
                                pNode,
                                SingleTextNode(")", Range.Zero),
                                Range.Zero
                            )
                        ))

                    |> (fun pnodes ->
                        Pattern.LongIdent(
                            PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pnodes, Range.Zero)
                        ))

        )

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member inline Parameter(parameters: (string * Type option) list, hasTupled: bool) =

            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                Parameter.Parameters.WithValue(parameters),
                Parameter.HasTupledParameters.WithValue(hasTupled)
            )


        static member inline PatternWithCurriedParameters(parameters: (string * Type option) list) =

            Ast.Parameter(parameters, false) |> Tree.compile

        static member inline PatternWithTupledParameters(parameters: (string * Type option) list) =

            Ast.Parameter(parameters, true) |> Tree.compile
