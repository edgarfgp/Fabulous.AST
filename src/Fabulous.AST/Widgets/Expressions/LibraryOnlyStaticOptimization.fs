namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module LibraryOnlyStaticOptimization =
    let OptimizedValue = Attributes.defineWidget "Value"

    let Value = Attributes.defineWidget "Value"

    let Constraints =
        Attributes.defineScalar<StaticOptimizationConstraint list> "Constraints"

    let WidgetKey =
        Widgets.register "LibraryOnlyStaticOptimization" (fun widget ->
            let optExpr = Widgets.getNodeFromWidget<Expr> widget OptimizedValue
            let expr = Widgets.getNodeFromWidget<Expr> widget Value
            let constraints = Widgets.getScalarValue widget Constraints

            Expr.LibraryOnlyStaticOptimization(
                ExprLibraryOnlyStaticOptimizationNode(optExpr, constraints, expr, Range.Zero)
            ))

[<AutoOpen>]
module LibraryOnlyStaticOptimizationBuilders =
    type Ast with

        static member LibraryOnlyStaticOptimizationExpr
            (
                optExpr: WidgetBuilder<Expr>,
                constraints: WidgetBuilder<StaticOptimizationConstraint> list,
                expr: WidgetBuilder<Expr>
            ) =
            let constraints = constraints |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                LibraryOnlyStaticOptimization.WidgetKey,
                AttributesBundle(
                    StackList.one(LibraryOnlyStaticOptimization.Constraints.WithValue(constraints)),
                    [| LibraryOnlyStaticOptimization.OptimizedValue.WithValue(optExpr.Compile())
                       LibraryOnlyStaticOptimization.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member LibraryOnlyStaticOptimizationExpr
            (
                optExpr: WidgetBuilder<Constant>,
                constraints: WidgetBuilder<StaticOptimizationConstraint> list,
                expr: WidgetBuilder<Expr>
            ) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, constraints, expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: WidgetBuilder<Expr>, expr: WidgetBuilder<Expr>) =
            Ast.LibraryOnlyStaticOptimizationExpr(optExpr, [], expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: WidgetBuilder<Expr>, expr: string) =
            Ast.LibraryOnlyStaticOptimizationExpr(optExpr, [], Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: WidgetBuilder<Constant>, expr: string) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: string, expr: WidgetBuilder<Expr>) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], expr)

        static member LibraryOnlyStaticOptimizationExpr
            (optExpr: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>)
            =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: string, expr: WidgetBuilder<Constant>) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr(optExpr: string, expr: string) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, [], Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr
            (optExpr: string, constraints: WidgetBuilder<StaticOptimizationConstraint> list, expr: WidgetBuilder<Expr>) =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, constraints, expr)

        static member LibraryOnlyStaticOptimizationExpr
            (
                optExpr: WidgetBuilder<Expr>,
                constraints: WidgetBuilder<StaticOptimizationConstraint> list,
                expr: WidgetBuilder<Constant>
            ) =
            Ast.LibraryOnlyStaticOptimizationExpr(optExpr, constraints, Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr
            (optExpr: WidgetBuilder<Expr>, constraints: WidgetBuilder<StaticOptimizationConstraint> list, expr: string) =
            Ast.LibraryOnlyStaticOptimizationExpr(optExpr, constraints, Ast.ConstantExpr expr)

        static member LibraryOnlyStaticOptimizationExpr
            (optExpr: string, constraints: WidgetBuilder<StaticOptimizationConstraint> list, expr: string)
            =
            Ast.LibraryOnlyStaticOptimizationExpr(Ast.ConstantExpr optExpr, constraints, Ast.ConstantExpr(expr))
