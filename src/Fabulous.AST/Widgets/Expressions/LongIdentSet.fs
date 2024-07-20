namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module LongIdentSet =
    let Value = Attributes.defineWidget "SingleNode"

    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "LongIdentSet" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let identifier = Widgets.getScalarValue widget Identifier

            Expr.LongIdentSet(
                ExprLongIdentSetNode(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero),
                    expr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module LongIdentSetBuilders =
    type Ast with

        static member LongIdentSetExpr(identifier: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                LongIdentSet.WidgetKey,
                AttributesBundle(
                    StackList.one(LongIdentSet.Identifier.WithValue(identifier)),
                    [| LongIdentSet.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member LongIdentSetExpr(identifier: string, value: WidgetBuilder<Constant>) =
            Ast.LongIdentSetExpr(identifier, Ast.ConstantExpr(value))

        static member LongIdentSetExpr(identifier: string, value: string) =
            Ast.LongIdentSetExpr(identifier, Ast.Constant(value))
