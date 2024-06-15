namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string> "Dollars"
    let Value = Attributes.defineScalar<string> "First"
    let ExprVal = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars =
                Widgets.tryGetScalarValue widget Dollars |> ValueOption.defaultValue "$"

            let value = Widgets.tryGetScalarValue widget Value |> ValueOption.defaultValue ""

            let expr = Widgets.tryGetNodeFromWidget<Expr> widget ExprVal

            let parts =
                [ match expr with
                  | ValueNone ->
                      Choice1Of2(SingleTextNode.Create(dollars))
                      Choice1Of2(SingleTextNode.doubleQuote)
                      Choice1Of2(SingleTextNode.Create(value))
                      Choice1Of2(SingleTextNode.doubleQuote)
                  | ValueSome expr ->
                      Choice1Of2(SingleTextNode.Create(dollars))
                      Choice1Of2(SingleTextNode.doubleQuote)
                      Choice1Of2(SingleTextNode.Create(value))
                      Choice1Of2(SingleTextNode.leftCurlyBrace)
                      Choice2Of2(FillExprNode(expr, None, Range.Zero))
                      Choice1Of2(SingleTextNode.rightCurlyBrace)
                      Choice1Of2(SingleTextNode.doubleQuote) ]

            Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(parts, Range.Zero)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with

        static member private BaseInterpolatedStringExpr
            (dollars: string voption, value: string voption, expr: WidgetBuilder<Expr> voption)
            =
            let scalars =
                match dollars, value with
                | ValueSome dollars, ValueSome value ->
                    StackList.two(
                        InterpolatedString.Dollars.WithValue(dollars),
                        InterpolatedString.Value.WithValue(value)
                    )
                | ValueSome dollars, ValueNone -> StackList.one(InterpolatedString.Dollars.WithValue(dollars))
                | ValueNone, ValueSome value -> StackList.one(InterpolatedString.Value.WithValue(value))
                | ValueNone, ValueNone -> StackList.empty()

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    scalars,
                    [| match expr with
                       | ValueNone -> ()
                       | ValueSome expr -> InterpolatedString.ExprVal.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member InterpolatedStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueNone, ValueSome expr)

        static member InterpolatedStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueNone, ValueSome(Ast.ConstantExpr(expr)))

        static member InterpolatedStringExpr(expr: string) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueNone, ValueSome(Ast.ConstantExpr(expr)))

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueSome value, ValueSome expr)

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Constant>) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueSome value, ValueSome(Ast.ConstantExpr(expr)))

        static member InterpolatedStringExpr(value: string, expr: string) =
            Ast.BaseInterpolatedStringExpr(ValueNone, ValueSome value, ValueSome(Ast.ConstantExpr(expr)))

        static member InterpolatedStringExpr(dollars: string, value: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(ValueSome dollars, ValueSome value, ValueSome expr)

        static member InterpolatedStringExpr(dollars: string, value: string, expr: WidgetBuilder<Constant>) =
            Ast.BaseInterpolatedStringExpr(ValueSome dollars, ValueSome value, ValueSome(Ast.ConstantExpr(expr)))

        static member InterpolatedStringExpr(dollars: string, value: string, expr: string) =
            Ast.BaseInterpolatedStringExpr(ValueSome dollars, ValueSome value, ValueSome(Ast.ConstantExpr(expr)))
