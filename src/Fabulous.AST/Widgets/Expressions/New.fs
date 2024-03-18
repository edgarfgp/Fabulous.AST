namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module New =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let TypeVal = Attributes.defineScalar<StringOrWidget<Type>> "Type"

    let WidgetKey =
        Widgets.register "New" (fun widget ->
            let expr = Widgets.getScalarValue widget Value

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let typ = Widgets.getScalarValue widget TypeVal

            let typ =
                match typ with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

[<AutoOpen>]
module NewBuilders =
    type Ast with

        static member NewExpr(t: WidgetBuilder<Type>, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        New.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        New.TypeVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak t))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member NewExpr(t: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        New.TypeVal.WithValue(StringOrWidget.StringExpr(Unquoted t)),
                        New.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member NewExpr(t: string, value: StringVariant) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        New.TypeVal.WithValue(StringOrWidget.StringExpr(Unquoted t)),
                        New.Value.WithValue(StringOrWidget.StringExpr(value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
