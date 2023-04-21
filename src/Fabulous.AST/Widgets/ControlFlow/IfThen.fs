namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let ThenExpr = Attributes.defineWidgetCollection "ThenExpr"
    let IfExpr = Attributes.defineWidget "IfExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr

            let thenExpr =
                Helpers.getNodesFromWidgetCollection<ComputationExpressionStatement> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                ifExpr,
                SingleTextNode("then", Range.Zero),
                Expr.CompExprBody(ExprCompExprBodyNode(thenExpr, Range.Zero)),
                Range.Zero
            ))

[<AutoOpen>]
module IfThenBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThen(exp: WidgetBuilder<Expr>) =
            CollectionBuilder<ExprIfThenNode, ComputationExpressionStatement>(
                IfThen.WidgetKey,
                IfThen.ThenExpr,
                AttributesBundle(StackList.empty(), ValueSome [| IfThen.IfExpr.WithValue(exp.Compile()) |], ValueNone)
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
