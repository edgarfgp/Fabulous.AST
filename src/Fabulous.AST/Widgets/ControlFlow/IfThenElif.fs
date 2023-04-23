namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    let IfTenElifExpr = Attributes.defineScalar<ExprIfThenNode list> "IfTenElifExpr"

    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr

            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            let ifTenElifExpr = Helpers.tryGetScalarValue widget IfTenElifExpr

            let ifTenElifExpr =
                match ifTenElifExpr with
                | ValueSome ifTenElifExpr -> ifTenElifExpr
                | ValueNone -> []

            let elseExpr = Helpers.tryGetNodeFromWidget<Expr> widget ElseExpr

            let elseExpr =
                match elseExpr with
                | ValueSome elseExpr -> Some(SingleTextNode("else", Range.Zero), elseExpr)
                | ValueNone -> None

            ExprIfThenElifNode(
                [ ExprIfThenNode(
                      IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                      ifExpr,
                      SingleTextNode("then", Range.Zero),
                      thenExpr,
                      Range.Zero
                  )
                  for elifExpr in ifTenElifExpr do
                      elifExpr ],
                elseExpr,
                Range.Zero
            ))

[<AutoOpen>]
module IfThenElifBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThen(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprIfThenElifNode>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThenElif.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElif.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElIf
            (
                ifExpr: WidgetBuilder<Expr>,
                thenExpr: WidgetBuilder<Expr>,
                elifExpr: ExprIfThenNode list
            ) =
            WidgetBuilder<ExprIfThenElifNode>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.IfTenElifExpr.WithValue(elifExpr)),
                    ValueSome
                        [| IfThenElif.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElif.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElIfElse
            (
                ifExpr: WidgetBuilder<Expr>,
                thenExpr: WidgetBuilder<Expr>,
                elifExpr: ExprIfThenNode list,
                elseExpr: WidgetBuilder<Expr>
            ) =
            WidgetBuilder<ExprIfThenElifNode>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.IfTenElifExpr.WithValue(elifExpr)),
                    ValueSome
                        [| IfThenElif.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElif.ThenExpr.WithValue(thenExpr.Compile())
                           IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElse
            (
                ifExpr: WidgetBuilder<Expr>,
                thenExpr: WidgetBuilder<Expr>,
                elseExpr: WidgetBuilder<Expr>
            ) =
            WidgetBuilder<ExprIfThenElifNode>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThenElif.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElif.ThenExpr.WithValue(thenExpr.Compile())
                           IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type IfThenIfYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ExprIfThenElifNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let expIfThen = Expr.IfThenElif(node)
        let moduleDecl = ModuleDecl.DeclExpr expIfThen
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
