namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ChainLink =
    let FuncName = Attributes.defineWidget "Value"
    let ParenName = Attributes.defineWidget "ParenName"
    let Identifier = Attributes.defineWidget "Identifier"
    let Item = Attributes.defineScalar<string> "Item"

    let WidgetIdentifierKey =
        Widgets.register "ChainLink_Identifier" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Expr> widget Identifier
            ChainLink.Identifier(identifier))

    let WidgetDotKey =
        Widgets.register "ChainLink_Dot" (fun _ -> ChainLink.Dot(SingleTextNode.dot))

    let WidgetExprKey =
        Widgets.register "ChainLink_Expr" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Expr> widget Identifier
            ChainLink.Expr(identifier))

    let WidgetAppParenKey =
        Widgets.register "ChainLink_AppParen" (fun widget ->
            let funcName = Widgets.getNodeFromWidget<Expr> widget FuncName
            let parenName = Widgets.getNodeFromWidget<Expr> widget ParenName

            ChainLink.AppParen(
                LinkSingleAppParen(
                    funcName,
                    ExprParenNode(
                        SingleTextNode.leftParenthesis,
                        parenName,
                        SingleTextNode.rightParenthesis,
                        Range.Zero
                    ),
                    Range.Zero
                )
            ))

    let WidgetAppUnitKey =
        Widgets.register "ChainLink_AppUnit" (fun widget ->
            let funcName = Widgets.getNodeFromWidget<Expr> widget FuncName

            ChainLink.AppUnit(
                LinkSingleAppUnit(
                    funcName,
                    UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero),
                    Range.Zero
                )
            ))

    let WidgetIndexKey =
        Widgets.register "ChainLink_Index" (fun widget ->
            let funcName = Widgets.getNodeFromWidget<Expr> widget FuncName
            ChainLink.IndexExpr(funcName))

[<AutoOpen>]
module ChainLinkBuilders =
    type Ast with

        static member ChainLinkIdentifier(value: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetIdentifierKey,
                AttributesBundle(StackList.empty(), [| ChainLink.Identifier.WithValue(value.Compile()) |], Array.empty)
            )

        static member ChainLinkIdentifier(value: WidgetBuilder<Constant>) =
            Ast.ChainLinkIdentifier(Ast.ConstantExpr(value))

        static member ChainLinkIdentifier(value: string) =
            Ast.ChainLinkIdentifier(Ast.Constant(value))

        static member ChainLinkDot() =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetDotKey,
                AttributesBundle(StackList.empty(), Array.empty, Array.empty)
            )

        static member ChainLinkExpr(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetExprKey,
                AttributesBundle(StackList.empty(), [| ChainLink.Identifier.WithValue(expr.Compile()) |], Array.empty)
            )

        static member ChainLinkExpr(expr: WidgetBuilder<Constant>) =
            Ast.ChainLinkExpr(Ast.ConstantExpr(expr))

        static member ChainLinkExpr(expr: string) = Ast.ChainLinkExpr(Ast.Constant(expr))

        static member ChainLinkAppParen(funcName: WidgetBuilder<Expr>, parenName: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetAppParenKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ChainLink.FuncName.WithValue(funcName.Compile())
                       ChainLink.ParenName.WithValue(parenName.Compile()) |],
                    Array.empty
                )
            )

        static member ChainLinkAppParen(funcName: WidgetBuilder<Constant>, parenName: WidgetBuilder<Expr>) =
            Ast.ChainLinkAppParen(Ast.ConstantExpr(funcName), parenName)

        static member ChainLinkAppParen(funcName: WidgetBuilder<Expr>, parenName: WidgetBuilder<Constant>) =
            Ast.ChainLinkAppParen(funcName, Ast.ConstantExpr(parenName))

        static member ChainLinkAppParen(funcName: WidgetBuilder<Constant>, parenName: WidgetBuilder<Constant>) =
            Ast.ChainLinkAppParen(Ast.ConstantExpr(funcName), Ast.ConstantExpr(parenName))

        static member ChainLinkAppParen(funcName: string, parenName: WidgetBuilder<Expr>) =
            Ast.ChainLinkAppParen(Ast.ConstantExpr(funcName), parenName)

        static member ChainLinkAppParen(funcName: WidgetBuilder<Expr>, parenName: string) =
            Ast.ChainLinkAppParen(funcName, Ast.ConstantExpr(parenName))

        static member ChainLinkAppParen(funcName: string, parenName: string) =
            Ast.ChainLinkAppParen(Ast.ConstantExpr(funcName), Ast.ConstantExpr(parenName))

        static member ChainLinkAppUnit(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetAppUnitKey,
                AttributesBundle(StackList.empty(), [| ChainLink.FuncName.WithValue(expr.Compile()) |], Array.empty)
            )

        static member ChainLinkAppUnit(expr: WidgetBuilder<Constant>) =
            Ast.ChainLinkAppUnit(Ast.ConstantExpr(expr))

        static member ChainLinkAppUnit(expr: string) =
            Ast.ChainLinkAppUnit(Ast.Constant(expr))

        static member ChainLinkIndex(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(
                ChainLink.WidgetIndexKey,
                AttributesBundle(StackList.empty(), [| ChainLink.FuncName.WithValue(expr.Compile()) |], Array.empty)
            )

        static member ChainLinkIndex(expr: WidgetBuilder<Constant>) =
            Ast.ChainLinkIndex(Ast.ConstantExpr(expr))

        static member ChainLinkIndex(expr: string) = Ast.ChainLinkIndex(Ast.Constant(expr))
