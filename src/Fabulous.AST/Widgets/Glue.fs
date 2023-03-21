namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

/// Glue are those widgets required to convert from one node type to another one,
/// but are not really useful to use by themselves
module Glue =
    let GluedWidget = Attributes.defineWidget "GluedWidget"
    
    let TopLevelBindingWidgetKey = Widgets.register "Glue.TopLevelBinding" (fun widget ->
        let gluedWidget = Helpers.getNodeFromWidget<BindingNode> widget GluedWidget
        ModuleDecl.TopLevelBinding gluedWidget
    )
    
    let DeclExprWidgetKey = Widgets.register "Glue.DeclExpr" (fun widget ->
        let gluedWidget = Helpers.getNodeFromWidget<Expr> widget GluedWidget
        ModuleDecl.DeclExpr gluedWidget
    )
    
    let UnitExprWidgetKey = Widgets.register "Glue.UnitExpr" (fun widget ->
        let gluedWidget = Helpers.getNodeFromWidget<UnitNode> widget GluedWidget
        Expr.Constant(Constant.Unit(gluedWidget))
    )

[<AutoOpen>]
module internal GlueBuilders =
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
            
        static member inline UnitExpr(exprWidget: WidgetBuilder<UnitNode>) =
            WidgetBuilder<Expr>(
                Glue.UnitExprWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Glue.GluedWidget.WithValue(exprWidget.Compile()) |],
                    ValueNone
                )
            )
            
type Expr =
    static member inline For(x: WidgetBuilder<UnitNode>) =
        Ast.UnitExpr(x)