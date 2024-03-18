namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IsInst =

    let Token = Attributes.defineScalar<SingleTextNode> "Token"

    let InstType = Attributes.defineScalar<StringOrWidget<Type>> "InstType"

    let WidgetKey =
        Widgets.register "IsInst" (fun widget ->
            let token = Widgets.getScalarValue widget Token
            let instType = Widgets.getScalarValue widget InstType

            let instType =
                match instType with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            Pattern.IsInst(PatIsInstNode(token, instType, Range.Zero)))

[<AutoOpen>]
module IsInstPatBuilders =
    type Ast with

        static member IsInstPat(value: WidgetBuilder<Type>) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IsInst.InstType.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        IsInst.Token.WithValue(SingleTextNode.isInstance)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IsInstPat(value: string) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IsInst.InstType.WithValue(StringOrWidget.StringExpr(Unquoted value)),
                        IsInst.Token.WithValue(SingleTextNode.isInstance)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IsInstPat(token: string, value: string) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IsInst.Token.WithValue(SingleTextNode.Create(token)),
                        IsInst.InstType.WithValue(StringOrWidget.StringExpr(Unquoted value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
