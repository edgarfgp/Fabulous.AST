namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    // let Parameters = Attributes.defineWidgetCollection "Parameters"
    // let HasTupledParameters = Attributes.defineScalar<bool> "HasTupledParameters"
    let Value = Attributes.defineWidget "Value"
    // let Type = Attributes.defineScalar<Type option> "Type"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let value = Helpers.getNodeFromWidget widget Value
            value)

// let WidgetKey =
//     Widgets.register "Pattern" (fun widget ->
//         let values = Helpers.getNodesFromWidgetCollection<Pattern> widget Parameters
//         let isTupled = Helpers.getScalarValue widget HasTupledParameters
//
//         if isTupled then
//             match values with
//             | [] ->
//                 Pattern.LongIdent(PatLongIdentNode(None, IdentListNode([], Range.Zero), None, values, Range.Zero))
//             | [ pNode ] ->
//                 match pNode with
//                 | Pattern.Parameter(pParamNode) when pParamNode.Type.IsSome ->
//                     Pattern.Paren(
//                         PatParenNode(
//                             SingleTextNode.leftParenthesis,
//                             pNode,
//                             SingleTextNode.rightParenthesis,
//                             Range.Zero
//                         )
//                     )
//                 | pNode ->
//                     Pattern.LongIdent(
//                         PatLongIdentNode(None, IdentListNode([], Range.Zero), None, [ pNode ], Range.Zero)
//                     )
//             | patterns ->
//                 patterns
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
//         else
//             match values with
//             | [] ->
//                 Pattern.LongIdent(PatLongIdentNode(None, IdentListNode([], Range.Zero), None, values, Range.Zero))
//             | [ pNode ] ->
//                 Pattern.LongIdent(
//                     PatLongIdentNode(None, IdentListNode([], Range.Zero), None, [ pNode ], Range.Zero)
//                 )
//             | patterns ->
//                 patterns
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
//                     Pattern.LongIdent(
//                         PatLongIdentNode(None, IdentListNode([], Range.Zero), None, pNodes, Range.Zero)
//                     )))

[<AutoOpen>]
module PatternBuilders =
    type Ast with

        static member private BasePattern(value: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Pattern.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Pattern.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member UnitPat() =
            Ast.BasePattern(
                Ast.EscapeHatch(
                    Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
                )
            )

        static member NamedPat(value: string) =
            Ast.BasePattern(
                Ast.EscapeHatch(Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero)))
            )

        static member NullPat() =
            Ast.BasePattern(Ast.EscapeHatch(Pattern.Null(SingleTextNode.``null``)))

        static member WildPat() =
            Ast.BasePattern(Ast.EscapeHatch(Pattern.Wild(SingleTextNode.underscore)))

        static member ConstantPat(value: Constant) =
            Ast.BasePattern(Ast.EscapeHatch(Pattern.Const(value)))

        static member ConstantPat(value: string) =
            Ast.ConstantPat(Constant.FromText(SingleTextNode.Create(value)))

// static member inline Parameters(isTupled: bool) =
//     CollectionBuilder<Pattern, Pattern>(
//         Pattern.WidgetKey,
//         Pattern.Parameters,
//         Pattern.HasTupledParameters.WithValue(isTupled)
//     )
//
// static member inline Parameters() = Ast.Parameters(false)
//     WidgetBuilder<Pattern>(
//         Parameters.WidgetKey,
//         Parameters.Parameters.WithValue((parameters, false)),
//         Parameters.HasTupledParameters.WithValue(hasTupled)
//     )

// static member inline MemberParameters(parameters: (string * Type option) list, hasTupled: bool) =
//     WidgetBuilder<Pattern>(
//         Parameters.WidgetKey,
//         Parameters.Parameters.WithValue((parameters, true)),
//         Parameters.HasTupledParameters.WithValue(hasTupled)
//     )


[<Extension>]
type ParametersYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Pattern>, x: WidgetBuilder<Pattern>) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
