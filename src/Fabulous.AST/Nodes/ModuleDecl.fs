namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabModuleDecl = interface end

module ModuleDecl =
    let BindingNode = Attributes.defineWidget "BindingNode"
    let Expr = Attributes.defineWidget "Expr"
    
    let TopLevelBindingWidgetKey = Widgets.register "ModuleDecl.TopLevelBinding" (fun widget ->
        let bindingNode = Helpers.getNodeFromWidget<BindingNode> widget BindingNode
        ModuleDecl.TopLevelBinding bindingNode
    )
    
    let DeclExprWidgetKey = Widgets.register "ModuleDecl.DeclExpr" (fun widget ->
        let expr = Helpers.getNodeFromWidget<Expr> widget Expr
        ModuleDecl.DeclExpr expr
    )
    
[<AutoOpen>]
module ModuleDeclBuilders =
    type Fabulous.AST.Ast with
        static member inline TopLevelBinding(binding: WidgetBuilder<#IFabBinding>) =
            WidgetBuilder<IFabModuleDecl>(
                ModuleDecl.TopLevelBindingWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| ModuleDecl.BindingNode.WithValue(binding.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline DeclExpr(expr: WidgetBuilder<#IFabExpr>) =
            WidgetBuilder<IFabModuleDecl>(
                ModuleDecl.DeclExprWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| ModuleDecl.Expr.WithValue(expr.Compile()) |],
                    ValueNone
                )
            )
    