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
