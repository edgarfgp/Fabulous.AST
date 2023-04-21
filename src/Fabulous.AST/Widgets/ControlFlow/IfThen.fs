namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr

            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                ifExpr,
                SingleTextNode("then", Range.Zero),
                thenExpr,
                Range.Zero
            ))

[<AutoOpen>]
module IfThenBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThen(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprIfThenNode>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThen.IfExpr.WithValue(ifExpr.Compile())
                           IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
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
