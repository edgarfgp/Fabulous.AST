namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElse =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElse" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr
            let elseExpr = Helpers.getNodeFromWidget<Expr> widget ElseExpr

            ExprIfThenElseNode(
                IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                ifExpr,
                SingleTextNode("then", Range.Zero),
                thenExpr,
                SingleTextNode("else", Range.Zero),
                elseExpr,
                Range.Zero
            ))

[<AutoOpen>]
module IfThenElseBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThenElse(ifExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>) =
            SingleChildBuilder<ExprIfThenElseNode, Expr>(
                IfThenElse.WidgetKey,
                IfThenElse.ThenExpr,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThenElse.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElse.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type IfThenElseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ExprIfThenElseNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let expIfThen = Expr.IfThenElse(node)
        let moduleDecl = ModuleDecl.DeclExpr expIfThen
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
