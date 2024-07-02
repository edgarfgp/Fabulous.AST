namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Expr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "ConstantExpr" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value
            Expr.Constant(value))

    let WidgetNullKey =
        Widgets.register "ExprNull" (fun _ -> Expr.Null(SingleTextNode.``null``))

[<AutoOpen>]
module ExprBuilders =
    type Ast with
        static member ConstantExpr(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Expr>(
                Expr.WidgetKey,
                AttributesBundle(StackList.empty(), [| Expr.Value.WithValue(value.Compile()) |], Array.empty)
            )

        static member ConstantExpr(value: string) = Ast.ConstantExpr(Ast.Constant(value))

        static member NullExpr() =
            WidgetBuilder<Expr>(Expr.WidgetNullKey, AttributesBundle(StackList.empty(), Array.empty, Array.empty))

type ExprYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Expr>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.DeclExpr node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
