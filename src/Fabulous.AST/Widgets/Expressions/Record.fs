namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RecordExpr =
    let Fields = Attributes.defineScalar<RecordFieldNode seq> "Fields"
    let CopyInfo = Attributes.defineWidget "CopyInfo"

    let OpenBrace = Attributes.defineScalar<SingleTextNode> "OpenBrace"

    let CloseBrace = Attributes.defineScalar<SingleTextNode> "CloseBrace"

    let WidgetKey =
        Widgets.register "RecordExpr" (fun widget ->
            let openBrace = Widgets.getScalarValue widget OpenBrace
            let closeBrace = Widgets.getScalarValue widget CloseBrace
            let copyInfo = Widgets.tryGetNodeFromWidget<Expr> widget CopyInfo

            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> Some copyInfo
                | ValueNone -> None

            let fields = Widgets.getScalarValue widget Fields |> List.ofSeq

            Expr.Record(ExprRecordNode(openBrace, copyInfo, fields, closeBrace, Range.Zero)))

[<AutoOpen>]
module RecordExprBuilders =
    type Ast with

        static member private BaseRecordExpr
            (
                copyInfo: WidgetBuilder<Expr> voption,
                leftSingleNode: SingleTextNode,
                rightSingleNode: SingleTextNode,
                fields: WidgetBuilder<RecordFieldNode> seq
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
                        RecordExpr.Fields.WithValue(fields |> Seq.map Gen.mkOak)
                    ),
                    copyInfo,
                    Array.empty
                )
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBrace,
                SingleTextNode.rightCurlyBrace,
                fields
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.RecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member RecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.RecordExpr(Ast.Constant(copyInfo), fields)

        static member RecordExpr(fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(ValueNone, SingleTextNode.leftCurlyBrace, SingleTextNode.rightCurlyBrace, fields)

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.AnonRecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member AnonRecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.AnonRecordExpr(Ast.Constant(copyInfo), fields)

        static member AnonRecordExpr(fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueNone,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )
