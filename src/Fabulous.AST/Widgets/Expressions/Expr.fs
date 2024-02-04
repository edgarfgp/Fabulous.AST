namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Expr =
    let Value = Attributes.defineWidget "Value"

    let TypedValue = Attributes.defineScalar<struct (string * Type)> "TypedValue"

    let NewValueType = Attributes.defineScalar<Type> "NewValue"

    let Items = Attributes.defineWidgetCollection "TupleItems"

    let OpeningNode = Attributes.defineScalar<SingleTextNode> "OpeningNode"

    let ClosingNode = Attributes.defineScalar<SingleTextNode> "ClosingNode"

    let WidgetSingleExprKey =
        Widgets.register "SingleExpr" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            expr)

    let WidgetLazyKey =
        Widgets.register "Lazy" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            Expr.Lazy(ExprLazyNode(SingleTextNode.``lazy``, expr, Range.Zero)))

    let WidgetQuotedKey =
        Widgets.register "Quoted" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let opening = Helpers.getScalarValue widget OpeningNode
            let closing = Helpers.getScalarValue widget ClosingNode
            Expr.Quote(ExprQuoteNode(opening, expr, closing, Range.Zero)))

    let WidgetTypedKey =
        Widgets.register "Typed" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let struct (text, typ) = Helpers.getScalarValue widget TypedValue
            Expr.Typed(ExprTypedNode(expr, text, typ, Range.Zero)))

    let WidgetNewKey =
        Widgets.register "New" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let typ = Helpers.getScalarValue widget NewValueType
            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

    let WidgetTupleKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Expr> widget Items

            let value =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.Tuple(ExprTupleNode(value, Range.Zero)))

    let WidgetStructTupleKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Expr> widget Items

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.StructTuple(
                ExprStructTupleNode(
                    SingleTextNode.``struct``,
                    ExprTupleNode(values, Range.Zero),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

    let WidgetArrayOrListKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Expr> widget Items
            let openNode = Helpers.getScalarValue widget OpeningNode
            let closeNode = Helpers.getScalarValue widget ClosingNode
            Expr.ArrayOrList(ExprArrayOrListNode(openNode, values, closeNode, Range.Zero)))

// TODO - add the rest of the expressions
(*
| Record of ExprRecordNode
| InheritRecord of ExprInheritRecordNode
| AnonStructRecord of ExprAnonStructRecordNode
| ObjExpr of ExprObjExprNode
| While of ExprWhileNode
| For of ExprForNode
| ForEach of ExprForEachNode
| NamedComputation of ExprNamedComputationNode
| Computation of ExprComputationNode
| CompExprBody of ExprCompExprBodyNode
| JoinIn of ExprJoinInNode
| ParenLambda of ExprParenLambdaNode
| Lambda of ExprLambdaNode
| MatchLambda of ExprMatchLambdaNode
| Match of ExprMatchNode
| TraitCall of ExprTraitCallNode
| ParenILEmbedded of SingleTextNode
| ParenFunctionNameWithStar of ExprParenFunctionNameWithStarNode
| Paren of ExprParenNode
| Dynamic of ExprDynamicNode
| PrefixApp of ExprPrefixAppNode
| SameInfixApps of ExprSameInfixAppsNode
| InfixApp of ExprInfixAppNode
| IndexWithoutDot of ExprIndexWithoutDotNode
| AppLongIdentAndSingleParenArg of ExprAppLongIdentAndSingleParenArgNode
| AppSingleParenArg of ExprAppSingleParenArgNode
| AppWithLambda of ExprAppWithLambdaNode
| NestedIndexWithoutDot of ExprNestedIndexWithoutDotNode
| App of ExprAppNode
| TypeApp of ExprTypeAppNode
| TryWithSingleClause of ExprTryWithSingleClauseNode
| TryWith of ExprTryWithNode
| TryFinally of ExprTryFinallyNode
| IfThen of ExprIfThenNode
| IfThenElse of ExprIfThenElseNode
| IfThenElif of ExprIfThenElifNode
| Ident of SingleTextNode
| OptVar of ExprOptVarNode
| LongIdentSet of ExprLongIdentSetNode
| DotIndexedGet of ExprDotIndexedGetNode
| DotIndexedSet of ExprDotIndexedSetNode
| NamedIndexedPropertySet of ExprNamedIndexedPropertySetNode
| DotNamedIndexedPropertySet of ExprDotNamedIndexedPropertySetNode
| Set of ExprSetNode
| LibraryOnlyStaticOptimization of ExprLibraryOnlyStaticOptimizationNode
| InterpolatedStringExpr of ExprInterpolatedStringExprNode
| IndexRangeWildcard of SingleTextNode
| TripleNumberIndexRange of ExprTripleNumberIndexRangeNode
| IndexRange of ExprIndexRangeNode
| IndexFromEnd of ExprIndexFromEndNode
| Typar of SingleTextNode
| Chain of ExprChain
| DotLambda of ExprDotLambda
| BeginEnd of ExprBeginEndNode
*)

[<AutoOpen>]
module ExpressionsBuilders =
    type Ast with

        static member private BaseExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Expr.WidgetSingleExprKey,
                AttributesBundle(StackList.empty(), ValueSome [| Expr.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member Lazy(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Expr.WidgetLazyKey,
                AttributesBundle(StackList.empty(), ValueSome [| Expr.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member Single(leading: string, addSpace: bool, supportsStroustrup: bool, expr: Expr) =
            Ast.BaseExpr(
                Ast.EscapeHatch(
                    Expr.Single(
                        ExprSingleNode(SingleTextNode.Create(leading), addSpace, supportsStroustrup, expr, Range.Zero)
                    )
                )
            )

        static member Constant(value: Constant) =
            Ast.BaseExpr(Ast.EscapeHatch(Expr.Constant(value)))

        static member Constant(value: string) =
            Ast.Constant(Constant.FromText(SingleTextNode.Create(value)))

        static member Constant(value: WidgetBuilder<Expr>) = Ast.BaseExpr(value)

        static member UnitExpr() =
            Ast.Constant(
                Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
            )

        static member Null() = Expr.Null(SingleTextNode.``null``)

        static member Quote(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Expr.WidgetQuotedKey,
                AttributesBundle(
                    StackList.two(
                        Expr.OpeningNode.WithValue(SingleTextNode.leftQuotation),
                        Expr.ClosingNode.WithValue(SingleTextNode.rightQuotation)
                    ),
                    ValueSome [| Expr.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member Typed(value: WidgetBuilder<Expr>, operator: string, t: Type) =
            WidgetBuilder<Expr>(
                Expr.WidgetTypedKey,
                AttributesBundle(
                    StackList.one(Expr.TypedValue.WithValue(operator, t)),
                    ValueSome [| Expr.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member New(value: WidgetBuilder<Expr>, t: Type) =
            WidgetBuilder<Expr>(
                Expr.WidgetNewKey,
                AttributesBundle(
                    StackList.one(Expr.NewValueType.WithValue(t)),
                    ValueSome [| Expr.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member Tuple() =
            CollectionBuilder<Expr, Expr>(Expr.WidgetTupleKey, Expr.Items)

        static member StructTuple() =
            CollectionBuilder<Expr, Expr>(Expr.WidgetStructTupleKey, Expr.Items)

        static member List'() =
            CollectionBuilder<Expr, Expr>(
                Expr.WidgetArrayOrListKey,
                Expr.Items,
                Expr.OpeningNode.WithValue(SingleTextNode.leftBracket),
                Expr.ClosingNode.WithValue(SingleTextNode.rightBracket)
            )

        static member Array'() =
            CollectionBuilder<Expr, Expr>(
                Expr.WidgetArrayOrListKey,
                Expr.Items,
                Expr.OpeningNode.WithValue(SingleTextNode.leftArray),
                Expr.ClosingNode.WithValue(SingleTextNode.rightArray)
            )

[<Extension>]
type ExpressionsYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Expr>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
