namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let Branches = Attributes.defineWidgetCollection "ElifExpr"
    let Branch = Attributes.defineWidget "ElifExpr"

    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let branches =
                Helpers.tryGetNodesFromWidgetCollection<ExprIfThenNode> widget Branches

            let branches =
                match branches with
                | Some branches -> branches
                | None -> []

            let elseExpr = Helpers.tryGetNodeFromWidget<Expr> widget ElseExpr

            let elseExpr =
                match elseExpr with
                | ValueSome elseExpr -> Some(SingleTextNode.``else``, elseExpr)
                | ValueNone -> None

            ExprIfThenElifNode(branches, elseExpr, Range.Zero))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with
        static member inline ConditionalExpr(elseExpr: WidgetBuilder<Expr>) =
            CollectionBuilder<ExprIfThenElifNode, ExprIfThenNode>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline ConditionalExpr() =
            CollectionBuilder<ExprIfThenElifNode, ExprIfThenNode>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(StackList.empty(), ValueNone, ValueNone)
            )

[<Extension>]
type IfThenIfYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ExprIfThenElifNode>
        ) : CollectionContent =
        let node = Gen.ast x
        let expIfThen = Expr.IfThenElif(node)
        let moduleDecl = ModuleDecl.DeclExpr expIfThen
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<ExprIfThenElifNode, ExprIfThenNode>,
            x: ExprIfThenNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
