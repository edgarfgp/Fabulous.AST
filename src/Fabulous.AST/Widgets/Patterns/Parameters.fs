namespace Fabulous.AST
//
// open Fabulous.AST.StackAllocatedCollections.StackList
// open Fantomas.Core.SyntaxOak
// open Fantomas.FCS.Text
//
// module Parameters =
//     let Parameters = Attributes.defineScalar<Pattern list> "Parameters"
//     let HasTupledParameters = Attributes.defineScalar<bool> "HasTupledParameters"
//
//     let WidgetKey =
//         Widgets.register "Parameters" (fun widget ->
//             let parameters = Widgets.getScalarValue widget Parameters
//             let isTupled = Widgets.getScalarValue widget HasTupledParameters
//
//             if isTupled then
//                 parameters
//                 |> List.map Choice1Of2
//                 |> List.intersperse(Choice2Of2(SingleTextNode.comma))
//                 |> (fun p -> PatTupleNode(p, Range.Zero))
//                 |> Pattern.Tuple
//                 |> (fun pNode ->
//                     Pattern.Paren(
//                         PatParenNode(
//                             SingleTextNode.leftParenthesis,
//                             pNode,
//                             SingleTextNode.rightParenthesis,
//                             Range.Zero
//                         )
//                     ))
//             else
//                 parameters
//                 |> List.map(fun pNode ->
//                     match pNode with
//                     | Pattern.Named nd -> Pattern.Named nd
//                     | pNode ->
//                         Pattern.Paren(
//                             PatParenNode(
//                                 SingleTextNode.leftParenthesis,
//                                 pNode,
//                                 SingleTextNode.rightParenthesis,
//                                 Range.Zero
//                             )
//                         ))
//                 |> (fun pNodes ->
//                     Pattern.LongIdent(PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pNodes, Range.Zero))))
//
// [<AutoOpen>]
// module ParametersBuilders =
//     type Ast with
//
//         static member private BaseParametersPat(parameters: WidgetBuilder<Pattern> list, isTupled: bool) =
//             let parameters = parameters |> List.map(Gen.mkOak)
//
//             WidgetBuilder<Pattern>(
//                 Parameters.WidgetKey,
//                 Parameters.Parameters.WithValue(parameters),
//                 Parameters.HasTupledParameters.WithValue(isTupled)
//             )
//
//         static member ParametersPat(parameters: WidgetBuilder<Pattern> list) =
//             Ast.BaseParametersPat(parameters, false)
//
//         static member ParametersPat(parameters: WidgetBuilder<Pattern> list, isTupled: bool) =
//             Ast.BaseParametersPat(parameters, isTupled)
