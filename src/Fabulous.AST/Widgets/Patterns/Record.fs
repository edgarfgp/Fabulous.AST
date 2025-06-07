namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RecordPat =
    let Fields = Attributes.defineScalar<PatRecordField seq> "Fields"

    let WidgetKey =
        Widgets.register "RecordPat" (fun widget ->
            let fields = Widgets.getScalarValue widget Fields |> List.ofSeq

            Pattern.Record(
                PatRecordNode(SingleTextNode.leftCurlyBrace, fields, SingleTextNode.rightCurlyBrace, Range.Zero)
            ))

[<AutoOpen>]
module RecordPatBuilders =
    type Ast with

        static member RecordPat(fields: WidgetBuilder<PatRecordField> seq) =
            WidgetBuilder<Pattern>(RecordPat.WidgetKey, RecordPat.Fields.WithValue(fields |> Seq.map Gen.mkOak))
