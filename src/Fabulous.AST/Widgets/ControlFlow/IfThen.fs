namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineWidget "Operator"
    let RightHandSide = Attributes.defineWidget "RightHandSide"
    let ThenExpr = Attributes.defineWidgetCollection "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let lhs = Helpers.getNodeFromWidget<SingleTextNode> widget LeftHandSide
            let operator = Helpers.getNodeFromWidget<SingleTextNode> widget Operator
            let rhs = Helpers.getNodeFromWidget<SingleTextNode> widget RightHandSide

            let thenExpr =
                Helpers.getNodesFromWidgetCollection<ComputationExpressionStatement> widget ThenExpr

            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                Expr.InfixApp(ExprInfixAppNode(Expr.Ident(lhs), operator, Expr.Ident(rhs), Range.Zero)),
                SingleTextNode("then", Range.Zero),
                Expr.CompExprBody(ExprCompExprBodyNode(thenExpr, Range.Zero)),
                Range.Zero
            ))

[<AutoOpen>]
module IfThenBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThen
            (
                lhs: WidgetBuilder<SingleTextNode>,
                operator: WidgetBuilder<SingleTextNode>,
                rhs: WidgetBuilder<SingleTextNode>
            ) =
            CollectionBuilder<ExprIfThenNode, ComputationExpressionStatement>(
                IfThen.WidgetKey,
                IfThen.ThenExpr,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThen.LeftHandSide.WithValue(lhs.Compile())
                           IfThen.Operator.WithValue(operator.Compile())
                           IfThen.RightHandSide.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThen(lhs: string, operator: string, rhs: string) =
            Ast.IfThen(
                Ast.EscapeHatch(SingleTextNode(lhs, Range.Zero)),
                Ast.EscapeHatch(SingleTextNode(operator, Range.Zero)),
                Ast.EscapeHatch(SingleTextNode(rhs, Range.Zero))
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
