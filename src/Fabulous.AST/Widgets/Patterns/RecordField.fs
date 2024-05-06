namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module RecordFieldPat =

    let Prefix = Attributes.defineScalar<string> "Prefix"

    let FieldName = Attributes.defineScalar<string> "FieldName"

    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "RecordFieldPat" (fun widget ->
            let fieldName =
                Widgets.getScalarValue widget FieldName
                |> PrettyNaming.NormalizeIdentifierBackticks

            let pattern = Widgets.getNodeFromWidget widget Pat

            let prefix =
                Widgets.tryGetScalarValue widget Prefix
                |> ValueOption.map(fun value ->
                    let value = PrettyNaming.NormalizeIdentifierBackticks value
                    Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero)))
                |> ValueOption.defaultValue None

            PatRecordField(prefix, SingleTextNode.Create(fieldName), SingleTextNode.equals, pattern, Range.Zero))

[<AutoOpen>]
module RecordFieldPatBuilders =
    type Ast with
        static member RecordFieldPat(name: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.one(RecordFieldPat.FieldName.WithValue(name)),
                    [| RecordFieldPat.Pat.WithValue(pat.Compile()) |],
                    Array.empty
                )
            )

        static member RecordFieldPat(name: string, pat: WidgetBuilder<Pattern>, prefix: string) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.two(RecordFieldPat.FieldName.WithValue(name), RecordFieldPat.Prefix.WithValue(prefix)),
                    [| RecordFieldPat.Pat.WithValue(pat.Compile()) |],
                    Array.empty
                )
            )
