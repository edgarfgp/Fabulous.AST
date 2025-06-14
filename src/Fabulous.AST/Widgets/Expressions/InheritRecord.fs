namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InheritRecord =
    let InheritConstructor = Attributes.defineWidget "InheritConstructor"
    let Fields = Attributes.defineScalar<RecordFieldNode seq> "Fields"

    let WidgetKey =
        Widgets.register "InheritRecord" (fun widget ->
            let inheritConstructor =
                Widgets.getNodeFromWidget<InheritConstructor> widget InheritConstructor

            let fields = Widgets.getScalarValue widget Fields |> List.ofSeq

            Expr.InheritRecord(
                ExprInheritRecordNode(
                    SingleTextNode.leftCurlyBrace,
                    inheritConstructor,
                    fields,
                    SingleTextNode.rightCurlyBrace,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module InheritRecordBuilders =
    type Ast with

        static member InheritRecordExpr
            (value: WidgetBuilder<InheritConstructor>, fields: WidgetBuilder<RecordFieldNode> seq)
            =
            WidgetBuilder<Expr>(
                InheritRecord.WidgetKey,
                AttributesBundle(
                    StackList.one(InheritRecord.Fields.WithValue(fields |> Seq.map Gen.mkOak)),
                    [| InheritRecord.InheritConstructor.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member InheritRecordExpr(value: WidgetBuilder<InheritConstructor>) = Ast.InheritRecordExpr(value, [])
