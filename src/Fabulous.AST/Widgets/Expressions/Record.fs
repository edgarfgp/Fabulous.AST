namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module RecordExpr =
    let Fields = Attributes.defineScalar<RecordFieldNode list> "Fields"
    let CopyInfo = Attributes.defineWidget "CopyInfo"

    let OpenBrace = Attributes.defineScalar<SingleTextNode> "OpenBrace"

    let CloseBrace = Attributes.defineScalar<SingleTextNode> "CloseBrace"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let openBrace = Widgets.getScalarValue widget OpenBrace
            let closeBrace = Widgets.getScalarValue widget CloseBrace
            let copyInfo = Widgets.tryGetNodeFromWidget<Expr> widget CopyInfo

            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> Some copyInfo
                | ValueNone -> None

            let fields = Widgets.getScalarValue widget Fields

            Expr.Record(ExprRecordNode(openBrace, copyInfo, fields, closeBrace, Range.Zero)))

[<AutoOpen>]
module RecordExprBuilders =
    type Ast with

        static member private BaseRecordExpr
            (
                copyInfo: WidgetBuilder<Expr> voption,
                leftSingleNode: SingleTextNode,
                rightSingleNode: SingleTextNode,
                fields: WidgetBuilder<RecordFieldNode> list
            ) =
            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> [| RecordExpr.CopyInfo.WithValue(copyInfo.Compile()) |]
                | ValueNone -> Array.empty

            WidgetBuilder<Expr>(
                RecordExpr.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        RecordExpr.OpenBrace.WithValue(leftSingleNode),
                        RecordExpr.CloseBrace.WithValue(rightSingleNode),
                        RecordExpr.Fields.WithValue(fields |> List.map Gen.mkOak)
                    ),
                    copyInfo,
                    Array.empty
                )
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBrace,
                SingleTextNode.rightCurlyBrace,
                fields
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.RecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member RecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.RecordExpr(Ast.Constant(copyInfo), fields)

        static member RecordExpr(fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.BaseRecordExpr(ValueNone, SingleTextNode.leftCurlyBrace, SingleTextNode.rightCurlyBrace, fields)

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.AnonRecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member AnonRecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.AnonRecordExpr(Ast.Constant(copyInfo), fields)

        static member AnonRecordExpr(fields: WidgetBuilder<RecordFieldNode> list) =
            Ast.BaseRecordExpr(
                ValueNone,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )
