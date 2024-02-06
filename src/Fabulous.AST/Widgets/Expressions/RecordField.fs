namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module RecordField =
    let RecordExpr = Attributes.defineWidget "RecordExpr"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "RecordField" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget RecordExpr
            let name = Helpers.getScalarValue widget Name

            RecordFieldNode(
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
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
                    StackList.one(RecordField.Name.WithValue(name)),
                    ValueSome [| RecordField.RecordExpr.WithValue(expr.Compile()) |],
                    ValueNone
                )
            )
