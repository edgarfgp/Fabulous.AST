namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Expr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Expr" (fun widget ->
            let value = Helpers.getNodeFromWidget<Expr> widget Value
            value)

[<AutoOpen>]
module ExprBuilders =
    type Ast with

        static member private BaseConstantExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Expr.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Expr.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member ConstantExpr(value: Constant) =
            Ast.BaseConstantExpr(Ast.EscapeHatch(Expr.Constant(value)))

        static member ConstantExpr(value: string) =
            Ast.ConstantExpr(Constant.FromText(SingleTextNode.Create(value)))

        static member UnitExpr() =
            Ast.ConstantExpr(
                Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
            )

[<Extension>]
type ExprYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Expr>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
