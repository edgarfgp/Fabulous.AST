namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"

    let WidgetIfThenKey =
        Widgets.register "IfThen" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode.``if``),
                ifExpr,
                SingleTextNode.``then``,
                thenExpr,
                Range.Zero
            ))

    let WidgetElIfThenKey =
        Widgets.register "ElIfThen" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode.``elif``),
                ifExpr,
                SingleTextNode.``then``,
                thenExpr,
                Range.Zero
            ))

    let WidgetElseIfThenKey =
        Widgets.register "ElIfThen" (fun widget ->
            let elseIfExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.ElseIf(ElseIfNode(Range.Zero, Range.Zero, Unchecked.defaultof<Node>, Range.Zero)),
                elseIfExpr,
                SingleTextNode.``then``,
                thenExpr,
                Range.Zero
            ))

[<AutoOpen>]
module IfThenBuilders =
    type Ast with

        static member inline IfThen(expr: WidgetBuilder<Expr>) =
            SingleChildBuilder<ExprIfThenNode, Expr>(
                IfThen.WidgetIfThenKey,
                IfThen.ThenExpr,
                AttributesBundle(StackList.empty(), ValueSome [| IfThen.IfExpr.WithValue(expr.Compile()) |], ValueNone)
            )

        static member inline ElIfThen(expr: WidgetBuilder<Expr>) =
            SingleChildBuilder<ExprIfThenNode, Expr>(
                IfThen.WidgetElIfThenKey,
                IfThen.ThenExpr,
                AttributesBundle(StackList.empty(), ValueSome [| IfThen.IfExpr.WithValue(expr.Compile()) |], ValueNone)
            )

        static member inline ElseIfThen(expr: WidgetBuilder<Expr>) =
            SingleChildBuilder<ExprIfThenNode, Expr>(
                IfThen.WidgetElseIfThenKey,
                IfThen.ThenExpr,
                AttributesBundle(StackList.empty(), ValueSome [| IfThen.IfExpr.WithValue(expr.Compile()) |], ValueNone)
            )

[<Extension>]
type IfThenYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ExprIfThenNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let expIfThen = Expr.IfThen(node)
        let moduleDecl = ModuleDecl.DeclExpr expIfThen
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
