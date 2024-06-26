namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PrefixApp =
    let Operator = Attributes.defineScalar<string> "Operator"

    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "PrefixApp" (fun widget ->
            let op = Widgets.getScalarValue widget Operator
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.PrefixApp(ExprPrefixAppNode(SingleTextNode.Create(op), expr, Range.Zero)))

[<AutoOpen>]
module PrefixAppBuilders =
    type Ast with

        static member PrefixAppExpr(operator: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                PrefixApp.WidgetKey,
                AttributesBundle(
                    StackList.one(PrefixApp.Operator.WithValue(operator)),
                    [| PrefixApp.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member PrefixAppExpr(operator: string, value: WidgetBuilder<Constant>) =
            Ast.PrefixAppExpr(operator, Ast.ConstantExpr value)

        static member PrefixAppExpr(operator: string, value: string) =
            Ast.PrefixAppExpr(operator, Ast.Constant(value))
