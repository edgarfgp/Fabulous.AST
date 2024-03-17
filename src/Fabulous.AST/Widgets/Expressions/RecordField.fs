namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module RecordField =
    let RecordExpr = Attributes.defineScalar<StringOrWidget<Expr>> "RecordExpr"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "RecordField" (fun widget ->
            let expr = Widgets.getScalarValue widget RecordExpr

            let expr =
                match expr with
                | StringOrWidget.StringExpr expr ->
                    let expr = StringParsing.normalizeIdentifierBackticks expr
                    Expr.Constant(Constant.FromText(SingleTextNode.Create(expr)))
                | StringOrWidget.WidgetExpr expr -> expr

            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks
                |> SingleTextNode.Create

            RecordFieldNode(
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                SingleTextNode.equals,
                expr,
                Range.Zero
            ))

[<AutoOpen>]
module RecordFieldBuilders =
    type Ast with

        static member inline RecordFieldExpr(name: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<RecordFieldNode>(
                RecordField.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        RecordField.Name.WithValue(name),
                        RecordField.RecordExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline RecordFieldExpr(name: string, expr: string) =
            WidgetBuilder<RecordFieldNode>(
                RecordField.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        RecordField.Name.WithValue(name),
                        RecordField.RecordExpr.WithValue(StringOrWidget.StringExpr(Unquoted expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
