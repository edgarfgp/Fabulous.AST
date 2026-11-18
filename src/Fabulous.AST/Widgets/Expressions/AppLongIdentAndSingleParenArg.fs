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

            // Fantomas 8 folded this into the general single-paren-argument application.
            Expr.AppSingleParenArg(
                ExprAppSingleParenArgNode(
                    Expr.OptVar(
                        ExprOptVarNode(
                            false,
                            IdentListNode(
                                [ for name in functionName do
                                      IdentifierOrDot.Ident(name) ],
                                Range.range0
                            ),
                            Range.range0
                        )
                    ),
                    expr,
                    Range.range0
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
