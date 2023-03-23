namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

/// Glue are those widgets required to convert from one node type to another one,
/// but are not really useful to use by themselves
module Glue =
    let GluedValue = Attributes.defineScalar<obj> "GluedValue"
    let GluedWidget = Attributes.defineWidget "GluedWidget"

    let TopLevelBindingWidgetKey =
        Widgets.register "Glue.TopLevelBinding" (fun widget ->
            let gluedWidget = Helpers.getNodeFromWidget<BindingNode> widget GluedWidget
            ModuleDecl.TopLevelBinding gluedWidget)

    let DeclExprWidgetKey =
        Widgets.register "Glue.DeclExpr" (fun widget ->
            let gluedWidget = Helpers.getNodeFromWidget<Expr> widget GluedWidget
            ModuleDecl.DeclExpr gluedWidget)

    let UnitConstantExprWidgetKey =
        Widgets.register "Glue.UnitConstantExpr" (fun widget ->
            let gluedWidget = Helpers.getNodeFromWidget<UnitNode> widget GluedWidget
            Expr.Constant(Constant.Unit(gluedWidget)))

    let TextConstantExprWidgetKey =
        Widgets.register "Glue.TextConstantExpr" (fun widget ->
            let gluedValue = Helpers.getScalarValue widget GluedValue |> string
            Expr.Constant(Constant.FromText(SingleTextNode(gluedValue, Range.Zero))))

[<AutoOpen>]
module GlueBuilders =
    type Fabulous.AST.Ast with

        static member inline TopLevelBinding(bindingWidget: WidgetBuilder<BindingNode>) =
            WidgetBuilder<ModuleDecl>(
                Glue.TopLevelBindingWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Glue.GluedWidget.WithValue(bindingWidget.Compile()) |],
                    ValueNone
                )
            )

        static member inline DeclExpr(exprWidget: WidgetBuilder<Expr>) =
            WidgetBuilder<ModuleDecl>(
                Glue.DeclExprWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Glue.GluedWidget.WithValue(exprWidget.Compile()) |],
                    ValueNone
                )
            )

        static member inline UnitConstantExpr(exprWidget: WidgetBuilder<UnitNode>) =
            WidgetBuilder<Expr>(
                Glue.UnitConstantExprWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Glue.GluedWidget.WithValue(exprWidget.Compile()) |],
                    ValueNone
                )
            )

        static member inline TextConstantExpr(text: string) =
            WidgetBuilder<Expr>(Glue.TextConstantExprWidgetKey, Glue.GluedValue.WithValue(text))

type Expr =
    static member inline For(x: WidgetBuilder<UnitNode>) = Ast.UnitConstantExpr(x)

    static member inline For(x: string) = Ast.TextConstantExpr(x)
