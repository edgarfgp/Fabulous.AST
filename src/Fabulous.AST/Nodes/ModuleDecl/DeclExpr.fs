namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabDeclExpr = inherit IFabModuleDecl

type IFabExpr = inherit IFabNodeBase

module DeclExpr =
    let Expr = Attributes.defineWidget "Expr"
    
    let WidgetKey = Widgets.register "DeclExpr" (fun (widget: Widget) ->
        let expr =
            Helpers.getWidgetValue widget Expr
            |> Helpers.createValueForWidget<Expr>
            
        ModuleDecl.DeclExpr expr
    )
    
[<AutoOpen>]
module DeclExprBuilders =
    type Fabulous.AST.Node with
        static member inline DeclExpr(expr: WidgetBuilder<#IFabExpr>) =
            WidgetBuilder<IFabDeclExpr>(
                DeclExpr.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| DeclExpr.Expr.WithValue(expr.Compile()) |],
                    ValueNone
                )
            )
            