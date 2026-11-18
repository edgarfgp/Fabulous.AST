namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

/// One link of a member-access / call chain, in source order: a receiver, then
/// `ChainLinkDot()` followed by one link per step, e.g. `A.B(c).D()`.
///
/// Fantomas 8 models a chain as a head plus dotted segments (<c>ExprChain</c>); a
/// <c>ChainExpr</c> converts these links into that shape when it is built.
[<RequireQualifiedAccess>]
type ChainLink =
    /// A name: `A` in `A.B`.
    | Identifier of Expr
    /// The `.` between two links.
    | Dot
    /// Any other expression used as a link.
    | Expr of Expr
    /// A call with a parenthesised argument: `C(d)`.
    | AppParen of functionName: Expr * argument: Expr
    /// A call with unit: `E()`.
    | AppUnit of functionName: Expr
    /// An index after a dot: `.[h]`.
    | IndexExpr of Expr

module ChainLink =
    let FuncName = Attributes.defineWidget "Value"
    let ParenName = Attributes.defineWidget "ParenName"
    let Identifier = Attributes.defineWidget "Identifier"
    let Item = Attributes.defineScalar<string> "Item"

    let WidgetIdentifierKey =
        Widgets.register "ChainLink_Identifier" (fun widget ->
            ChainLink.Identifier(Widgets.getNodeFromWidget<Expr> widget Identifier))

    let WidgetDotKey = Widgets.register "ChainLink_Dot" (fun _ -> ChainLink.Dot)

    let WidgetExprKey =
        Widgets.register "ChainLink_Expr" (fun widget ->
            ChainLink.Expr(Widgets.getNodeFromWidget<Expr> widget Identifier))

    let WidgetAppParenKey =
        Widgets.register "ChainLink_AppParen" (fun widget ->
            let funcName = Widgets.getNodeFromWidget<Expr> widget FuncName
            let parenName = Widgets.getNodeFromWidget<Expr> widget ParenName
            ChainLink.AppParen(funcName, parenName))

    let WidgetAppUnitKey =
        Widgets.register "ChainLink_AppUnit" (fun widget ->
            ChainLink.AppUnit(Widgets.getNodeFromWidget<Expr> widget FuncName))

    let WidgetIndexKey =
        Widgets.register "ChainLink_Index" (fun widget ->
            ChainLink.IndexExpr(Widgets.getNodeFromWidget<Expr> widget FuncName))

[<AutoOpen>]
module ChainLinkBuilders =
    type Ast with

        static member ChainLinkIdentifier(value: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(ChainLink.WidgetIdentifierKey, ChainLink.Identifier.WithValue(value.Compile()))

        static member ChainLinkIdentifier(value: WidgetBuilder<Constant>) =
            Ast.ChainLinkIdentifier(Ast.ConstantExpr(value))

        static member ChainLinkIdentifier(value: string) =
            Ast.ChainLinkIdentifier(Ast.Constant(value))

        static member ChainLinkDot() =
            WidgetBuilder<ChainLink>(ChainLink.WidgetDotKey)

        static member ChainLinkExpr(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(ChainLink.WidgetExprKey, ChainLink.Identifier.WithValue(expr.Compile()))

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
            WidgetBuilder<ChainLink>(ChainLink.WidgetAppUnitKey, ChainLink.FuncName.WithValue(expr.Compile()))

        static member ChainLinkAppUnit(expr: WidgetBuilder<Constant>) =
            Ast.ChainLinkAppUnit(Ast.ConstantExpr(expr))

        static member ChainLinkAppUnit(expr: string) =
            Ast.ChainLinkAppUnit(Ast.Constant(expr))

        static member ChainLinkIndex(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ChainLink>(ChainLink.WidgetIndexKey, ChainLink.FuncName.WithValue(expr.Compile()))

        static member ChainLinkIndex(expr: WidgetBuilder<Constant>) =
            Ast.ChainLinkIndex(Ast.ConstantExpr(expr))

        static member ChainLinkIndex(expr: string) = Ast.ChainLinkIndex(Ast.Constant(expr))
