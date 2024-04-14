namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RecordFieldPat =

    let Prefix = Attributes.defineScalar<StringVariant> "Prefix"

    let FieldName = Attributes.defineScalar<StringVariant> "OpenTextNode"

    let CloseTextNode = Attributes.defineScalar<SingleTextNode> "CloseTextNode"

    let Pat = Attributes.defineScalar<StringOrWidget<Pattern>> "Pat"

    let WidgetKey =
        Widgets.register "RecordFieldPat" (fun widget ->
            let fieldName = Widgets.getScalarValue widget FieldName
            let pattern = Widgets.getScalarValue widget Pat

            let pattern =
                match pattern with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierQuotes value
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero))
                | StringOrWidget.WidgetExpr pat -> pat

            let prefix =
                Widgets.tryGetScalarValue widget Prefix
                |> ValueOption.map(fun x ->
                    let x = StringParsing.normalizeIdentifierQuotes x
                    Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(x)) ], Range.Zero)))
                |> ValueOption.defaultValue None

            let fieldName = StringParsing.normalizeIdentifierQuotes fieldName

            PatRecordField(prefix, SingleTextNode.Create(fieldName), SingleTextNode.equals, pattern, Range.Zero))

[<AutoOpen>]
module RecordFieldPatBuilders =
    type Ast with
        static member RecordFieldPat(name: StringVariant, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        RecordFieldPat.FieldName.WithValue(name),
                        RecordFieldPat.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pat))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member RecordFieldPat(name: StringVariant, pat: WidgetBuilder<Pattern>, prefix: StringVariant) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        RecordFieldPat.FieldName.WithValue(name),
                        RecordFieldPat.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pat)),
                        RecordFieldPat.Prefix.WithValue(prefix)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member RecordFieldPat(name: StringVariant, pat: StringVariant) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        RecordFieldPat.FieldName.WithValue(name),
                        RecordFieldPat.Pat.WithValue(StringOrWidget.StringExpr(pat))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member RecordFieldPat(name: StringVariant, pat: StringVariant, prefix: StringVariant) =
            WidgetBuilder<PatRecordField>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        RecordFieldPat.FieldName.WithValue(name),
                        RecordFieldPat.Pat.WithValue(StringOrWidget.StringExpr(pat)),
                        RecordFieldPat.Prefix.WithValue(prefix)
                    ),
                    Array.empty,
                    Array.empty
                )
            )
