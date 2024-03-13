namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module New =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let Type = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "New" (fun widget ->
            let expr = Widgets.getScalarValue widget Value

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let typ = Widgets.getNodeFromWidget widget Type
            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

[<AutoOpen>]
module NewBuilders =
    type Ast with

        static member NewExpr(t: WidgetBuilder<Type>, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.one(New.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    ValueSome [| New.Type.WithValue(t.Compile()) |],
                    ValueNone
                )
            )

        static member NewExpr(t: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.one(New.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    ValueSome [| New.Type.WithValue(Ast.LongIdent(t).Compile()) |],
                    ValueNone
                )
            )

        static member NewExpr(t: string, value: string) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.one(New.Value.WithValue(StringOrWidget.StringExpr(value))),
                    ValueSome [| New.Type.WithValue(Ast.LongIdent(t).Compile()) |],
                    ValueNone
                )
            )
