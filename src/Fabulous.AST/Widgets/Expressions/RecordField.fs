namespace Fabulous.AST

open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module RecordField =
    let RecordExpr = Attributes.defineWidget "RecordExpr"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "RecordField" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget RecordExpr

            let name =
                Widgets.getScalarValue widget Name
                |> PrettyNaming.NormalizeIdentifierBackticks
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
                    StackList.one(RecordField.Name.WithValue(name)),
                    [| RecordField.RecordExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member inline RecordFieldExpr(name: string, expr: WidgetBuilder<Constant>) =
            Ast.RecordFieldExpr(name, Ast.ConstantExpr(expr))

        static member inline RecordFieldExpr(name: string, expr: string) =
            Ast.RecordFieldExpr(name, Ast.Constant(expr))
