namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

[<AutoOpen>]
module Patterns =

    type Pattern with

        static member CreateUnit() =
            Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))

        static member CreateNamed(name: string) =
            Pattern.Named(PatNamedNode(None, SingleTextNode.Create(name), Range.Zero))

        static member CreateSingleParameter(name: Pattern, pType: Type option) =
            Pattern.Paren(
                PatParenNode(
                    SingleTextNode.leftParenthesis,
                    Pattern.Parameter(PatParameterNode(None, name, pType, Range.Zero)),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )


        static member CreateParameter(name: Pattern, pType: Type option) =
            Pattern.Parameter(PatParameterNode(None, name, pType, Range.Zero))

        static member CreateTupleParameters(parameters: Pattern list) =
            [ parameters
              |> List.map Choice1Of2
              |> List.intersperse(Choice2Of2(SingleTextNode.comma))
              |> (fun p -> PatTupleNode(p, Range.Zero))
              |> Pattern.Tuple
              |> (fun pNode ->
                  Pattern.Paren(
                      PatParenNode(SingleTextNode.leftParenthesis, pNode, SingleTextNode.rightParenthesis, Range.Zero)
                  )) ]

        static member CreateCurriedParameters(parameters: Pattern list) =
            [ parameters
              |> List.map(fun pNode ->
                  Pattern.Paren(
                      PatParenNode(SingleTextNode.leftParenthesis, pNode, SingleTextNode.rightParenthesis, Range.Zero)
                  ))

              |> (fun pNodes ->
                  Pattern.LongIdent(PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pNodes, Range.Zero))) ]

module Parameters =

    let Parameters = Attributes.defineScalar<(string * Type option) list> "Parameters"
    let HasTupledParameters = Attributes.defineScalar<bool> "HasTupledParameters"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let parameters = Helpers.getScalarValue widget Parameters
            let hasTupledParameters = Helpers.getScalarValue widget HasTupledParameters

            match parameters with
            | [] -> Pattern.Unit(UnitNode(SingleTextNode.empty, SingleTextNode.empty, Range.Zero))
            | parameters ->
                let singleParamNodes =
                    parameters
                    |> List.map(fun (pName, pType) ->
                        match pType with
                        | None -> Pattern.Named(PatNamedNode(None, SingleTextNode.Create(pName), Range.Zero))
                        | Some _ ->
                            Pattern.Parameter(
                                PatParameterNode(
                                    None,
                                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(pName), Range.Zero)),
                                    pType,
                                    Range.Zero
                                )
                            )

                    )

                match hasTupledParameters with
                | true ->
                    match singleParamNodes with
                    | [ pNode ] ->
                        Pattern.LongIdent(
                            PatLongIdentNode(None, IdentListNode([], Range.Zero), None, [ pNode ], Range.Zero)
                        )
                    | patterns ->
                        patterns
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

                //curried params
                | false ->
                    match singleParamNodes with
                    | [ pNode ] ->
                        Pattern.LongIdent(
                            PatLongIdentNode(None, IdentListNode([], Range.Zero), None, [ pNode ], Range.Zero)
                        )
                    | patterns ->
                        patterns
                        |> List.map(fun pNode ->
                            Pattern.Paren(
                                PatParenNode(
                                    SingleTextNode.leftParenthesis,
                                    pNode,
                                    SingleTextNode.rightParenthesis,
                                    Range.Zero
                                )
                            ))
                        |> (fun pNodes ->
                            Pattern.LongIdent(
                                PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pNodes, Range.Zero)
                            )))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member inline Parameters(parameters: (string * Type option) list, hasTupled: bool) =
            WidgetBuilder<Pattern>(
                Parameters.WidgetKey,
                Parameters.Parameters.WithValue(parameters),
                Parameters.HasTupledParameters.WithValue(hasTupled)
            )
