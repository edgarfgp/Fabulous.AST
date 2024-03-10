namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module RecordExpr =
    let Fields = Attributes.defineWidgetCollection "Fields"
    let CopyInfo = Attributes.defineWidget "CopyInfo"

    let OpenBrace = Attributes.defineScalar<SingleTextNode> "OpenBrace"

    let CloseBrace = Attributes.defineScalar<SingleTextNode> "CloseBrace"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let openBrace = Helpers.getScalarValue widget OpenBrace
            let closeBrace = Helpers.getScalarValue widget CloseBrace
            let copyInfo = Helpers.tryGetNodeFromWidget<Expr> widget CopyInfo

            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> Some copyInfo
                | ValueNone -> None

            let fields = Helpers.getNodesFromWidgetCollection<RecordFieldNode> widget Fields

            Expr.Record(ExprRecordNode(openBrace, copyInfo, fields, closeBrace, Range.Zero)))

[<AutoOpen>]
module RecordExprBuilders =
    type Ast with

        static member private BaseRecordExpr
            (copyInfo: WidgetBuilder<Expr> voption, leftSingleNode: SingleTextNode, rightSingleNode: SingleTextNode) =
            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> ValueSome [| RecordExpr.CopyInfo.WithValue(copyInfo.Compile()) |]
                | ValueNone -> ValueNone

            CollectionBuilder<Expr, RecordFieldNode>(
                RecordExpr.WidgetKey,
                RecordExpr.Fields,
                AttributesBundle(
                    StackList.two(
                        RecordExpr.OpenBrace.WithValue(leftSingleNode),
                        RecordExpr.CloseBrace.WithValue(rightSingleNode)
                    ),
                    copyInfo,
                    ValueNone
                )
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Expr>) =
            Ast.BaseRecordExpr(ValueSome copyInfo, SingleTextNode.leftCurlyBrace, SingleTextNode.rightCurlyBrace)

        static member RecordExpr() =
            Ast.BaseRecordExpr(ValueNone, SingleTextNode.leftCurlyBrace, SingleTextNode.rightCurlyBrace)

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Expr>) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar
            )

        static member AnonRecordExpr() =
            Ast.BaseRecordExpr(ValueNone, SingleTextNode.leftCurlyBraceWithBar, SingleTextNode.rightCurlyBraceWithBar)

[<Extension>]
type RecordExprYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<Expr, RecordFieldNode>, x: WidgetBuilder<RecordFieldNode>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
