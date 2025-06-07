namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

open type Fabulous.AST.Ast

module AppLongIdentAndSingleParenArg =
    let ExprVal = Attributes.defineWidget "Name"
    let FunctionName = Attributes.defineScalar<string seq> "FunctionName"

    let WidgetKey =
        Widgets.register "AppLongIdentAndSingleParenArg" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget ExprVal

            let functionName =
                Widgets.getScalarValue widget FunctionName
                |> Seq.map(SingleTextNode.Create)
                |> Seq.intersperse(SingleTextNode.Create ".")

            Expr.AppLongIdentAndSingleParenArg(
                ExprAppLongIdentAndSingleParenArgNode(
                    IdentListNode(
                        [ for name in functionName do
                              IdentifierOrDot.Ident(name) ],
                        Range.Zero
                    ),
                    expr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module AppLongIdentAndSingleParenArgBuilders =
    type Ast with

        static member AppLongIdentAndSingleParenArgExpr(name: string seq, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                AppLongIdentAndSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.one(AppLongIdentAndSingleParenArg.FunctionName.WithValue(name)),
                    [| AppLongIdentAndSingleParenArg.ExprVal.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member AppLongIdentAndSingleParenArgExpr(name: string seq, expr: WidgetBuilder<Constant>) =
            Ast.AppLongIdentAndSingleParenArgExpr(name, Ast.ConstantExpr(expr))

        static member AppLongIdentAndSingleParenArgExpr(name: string seq, expr: string) =
            Ast.AppLongIdentAndSingleParenArgExpr(name, Ast.Constant(expr))

        static member AppLongIdentAndSingleParenArgExpr(name: string, expr: WidgetBuilder<Expr>) =
            Ast.AppLongIdentAndSingleParenArgExpr([ name ], expr)

        static member AppLongIdentAndSingleParenArgExpr(name: string, expr: WidgetBuilder<Constant>) =
            Ast.AppLongIdentAndSingleParenArgExpr(name, Ast.ConstantExpr expr)

        static member AppLongIdentAndSingleParenArgExpr(name: string, expr: string) =
            Ast.AppLongIdentAndSingleParenArgExpr(name, Ast.Constant expr)
