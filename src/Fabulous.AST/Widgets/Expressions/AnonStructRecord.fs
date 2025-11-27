namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AnonStructRecord =
    let Fields = Attributes.defineScalar<RecordFieldNode seq> "Fields"
    let CopyInfo = Attributes.defineWidget "CopyInfo"

    let WidgetKey =
        Widgets.register "AnonStructRecord" (fun widget ->
            let copyInfo = Widgets.tryGetNodeFromWidget<Expr> widget CopyInfo

            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> Some copyInfo
                | ValueNone -> None

            let fields = Widgets.getScalarValue widget Fields |> List.ofSeq

            Expr.AnonStructRecord(
                ExprAnonStructRecordNode(
                    SingleTextNode.``struct``,
                    SingleTextNode.leftCurlyBraceWithBar,
                    copyInfo,
                    fields,
                    SingleTextNode.rightCurlyBraceWithBar,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module AnonStructRecordBuilders =
    type Ast with

        static member private BaseAnonStructRecordExpr
            (copyInfo: WidgetBuilder<Expr> voption, fields: WidgetBuilder<RecordFieldNode> seq)
            =
            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> [| AnonStructRecord.CopyInfo.WithValue(copyInfo.Compile()) |]
                | ValueNone -> Array.empty

            WidgetBuilder<Expr>(
                AnonStructRecord.WidgetKey,
                AttributesBundle(
                    StackList.one(AnonStructRecord.Fields.WithValue(fields |> Seq.map Gen.mkOak)),
                    copyInfo,
                    Array.empty
                )
            )

        static member AnonStructRecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseAnonStructRecordExpr(ValueSome copyInfo, fields)

        static member AnonStructRecordExpr
            (copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> seq)
            =
            Ast.AnonStructRecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member AnonStructRecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.AnonStructRecordExpr(Ast.Constant(copyInfo), fields)

        static member AnonStructRecordExpr(fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseAnonStructRecordExpr(ValueNone, fields)
