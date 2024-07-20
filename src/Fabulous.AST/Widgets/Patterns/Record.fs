namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RecordPat =
    let Fields = Attributes.defineScalar<PatRecordField list> "Fields"

    let WidgetKey =
        Widgets.register "RecordPat" (fun widget ->
            let fields = Widgets.getScalarValue widget Fields

            Pattern.Record(
                PatRecordNode(SingleTextNode.leftCurlyBrace, fields, SingleTextNode.rightCurlyBrace, Range.Zero)
            ))

[<AutoOpen>]
module RecordPatBuilders =
    type Ast with

        static member RecordPat(fields: WidgetBuilder<PatRecordField> list) =
            WidgetBuilder<Pattern>(
                RecordPat.WidgetKey,
                AttributesBundle(
                    StackList.one(RecordPat.Fields.WithValue(fields |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
