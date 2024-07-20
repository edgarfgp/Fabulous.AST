namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module FillExprNode =
    let Value = Attributes.defineWidget "Value"

    let Identifier = Attributes.defineScalar<string> "Clauses"

    let WidgetKey =
        Widgets.register "FillExprNode" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value

            let identifier =
                Widgets.tryGetScalarValue widget Identifier
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(x)))
                |> ValueOption.defaultValue None

            FillExprNode(expr, identifier, Range.Zero))

[<AutoOpen>]
module FillExprNodeBuilders =
    type Ast with

        static member FillExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<FillExprNode>(
                FillExprNode.WidgetKey,
                AttributesBundle(StackList.empty(), [| FillExprNode.Value.WithValue(value.Compile()) |], Array.empty)
            )

        static member FillExpr(value: WidgetBuilder<Constant>) = Ast.FillExpr(Ast.ConstantExpr(value))

        static member FillExpr(value: string) = Ast.FillExpr(Ast.Constant(value))

        static member FillExpr(value: WidgetBuilder<Expr>, identifier: string) =
            WidgetBuilder<FillExprNode>(
                FillExprNode.WidgetKey,
                AttributesBundle(
                    StackList.one(FillExprNode.Identifier.WithValue(identifier)),
                    [| FillExprNode.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member FillExpr(value: WidgetBuilder<Constant>, identifier: string) =
            Ast.FillExpr(Ast.ConstantExpr(value), identifier)

        static member FillExpr(value: string, identifier: string) =
            Ast.FillExpr(Ast.Constant(value), identifier)
