namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module For =
    let Ident = Attributes.defineScalar<string> "Ident"

    let Start = Attributes.defineWidget "Start"

    let Direction = Attributes.defineScalar<bool> "Direction"

    let ToBody = Attributes.defineWidget "ToBody"

    let DoBody = Attributes.defineWidget "DoBody"

    let WidgetKey =
        Widgets.register "For" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident
            let start = Widgets.getNodeFromWidget widget Start
            let toBody = Widgets.getNodeFromWidget<Expr> widget ToBody
            let doBody = Widgets.getNodeFromWidget<Expr> widget DoBody
            let direction = Widgets.getScalarValue widget Direction

            Expr.For(
                ExprForNode(
                    SingleTextNode.``for``,
                    SingleTextNode.Create(ident),
                    SingleTextNode.equals,
                    start,
                    direction,
                    toBody,
                    doBody,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ForBuilders =
    type Ast with

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Expr>, toBody: WidgetBuilder<Expr>, doBody: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                For.WidgetKey,
                AttributesBundle(
                    StackList.two(For.Ident.WithValue(identifier), For.Direction.WithValue(true)),
                    [| For.Start.WithValue(start.Compile())
                       For.ToBody.WithValue(toBody.Compile())
                       For.DoBody.WithValue(doBody.Compile()) |],
                    Array.empty
                )
            )

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                toBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), toBody, doBody)

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                toBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr
            (identifier: string, start: string, toBody: WidgetBuilder<Expr>, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), toBody, doBody)

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                toBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForToExpr(identifier, start, Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                toBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForToExpr(identifier, start, Ast.ConstantExpr(toBody), Ast.ConstantExpr(doBody))

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                toBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(toBody), Ast.ConstantExpr(doBody))

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Expr>, toBody: string, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForToExpr(identifier, start, Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                toBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForToExpr(identifier, start, toBody, Ast.ConstantExpr(doBody))

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Expr>, toBody: WidgetBuilder<Expr>, doBody: string)
            =
            Ast.ForToExpr(identifier, start, toBody, Ast.ConstantExpr(doBody))

        static member ForToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                toBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), toBody, Ast.ConstantExpr(doBody))

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Constant>, toBody: WidgetBuilder<Expr>, doBody: string)
            =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), toBody, Ast.Constant(doBody))

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Constant>, toBody: string, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr
            (identifier: string, start: WidgetBuilder<Constant>, toBody: string, doBody: WidgetBuilder<Constant>) =
            Ast.ForToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(toBody), Ast.ConstantExpr(doBody))

        static member ForToExpr(identifier: string, start: WidgetBuilder<Constant>, toBody: string, doBody: string) =
            Ast.ForToExpr(identifier, start, toBody, Ast.Constant(doBody))

        static member ForToExpr
            (identifier: string, start: string, toBody: WidgetBuilder<Expr>, doBody: WidgetBuilder<Constant>)
            =
            Ast.ForToExpr(identifier, Ast.Constant(start), toBody, Ast.ConstantExpr(doBody))

        static member ForToExpr(identifier: string, start: string, toBody: WidgetBuilder<Expr>, doBody: string) =
            Ast.ForToExpr(identifier, Ast.Constant(start), toBody, Ast.Constant(doBody))

        static member ForToExpr
            (identifier: string, start: string, toBody: WidgetBuilder<Constant>, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr(identifier: string, start: string, toBody: WidgetBuilder<Constant>, doBody: string) =
            Ast.ForToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(toBody), Ast.Constant(doBody))

        static member ForToExpr(identifier: string, start: string, toBody: string, doBody: WidgetBuilder<Expr>) =
            Ast.ForToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(toBody), doBody)

        static member ForToExpr(identifier: string, start: string, toBody: string, doBody: WidgetBuilder<Constant>) =
            Ast.ForToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(toBody), Ast.ConstantExpr(doBody))

        static member ForToExpr(identifier: string, start: string, toBody: string, doBody: string) =
            Ast.ForToExpr(identifier, start, toBody, Ast.Constant(doBody))

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                downtoBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Expr>
            ) =
            WidgetBuilder<Expr>(
                For.WidgetKey,
                AttributesBundle(
                    StackList.two(For.Ident.WithValue(identifier), For.Direction.WithValue(false)),
                    [| For.Start.WithValue(start.Compile())
                       For.ToBody.WithValue(downtoBody.Compile())
                       For.DoBody.WithValue(doBody.Compile()) |],
                    Array.empty
                )
            )

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                downtoBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), downtoBody, doBody)

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                downtoBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                downtoBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForDownToExpr(
                identifier,
                Ast.ConstantExpr(start),
                Ast.ConstantExpr(downtoBody),
                Ast.ConstantExpr(doBody)
            )

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: WidgetBuilder<Expr>, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), downtoBody, doBody)

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                downtoBody: WidgetBuilder<Constant>,
                doBody: WidgetBuilder<Expr>
            ) =
            Ast.ForDownToExpr(identifier, start, Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Expr>, downtoBody: string, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForDownToExpr(identifier, start, Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Expr>,
                downtoBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForDownToExpr(identifier, start, downtoBody, Ast.ConstantExpr(doBody))

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Expr>, downtoBody: WidgetBuilder<Expr>, doBody: string)
            =
            Ast.ForDownToExpr(identifier, start, downtoBody, Ast.ConstantExpr(doBody))

        static member ForDownToExpr
            (
                identifier: string,
                start: WidgetBuilder<Constant>,
                downtoBody: WidgetBuilder<Expr>,
                doBody: WidgetBuilder<Constant>
            ) =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), downtoBody, Ast.ConstantExpr(doBody))

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Constant>, downtoBody: WidgetBuilder<Expr>, doBody: string) =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), downtoBody, Ast.Constant(doBody))

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Constant>, downtoBody: string, doBody: WidgetBuilder<Expr>) =
            Ast.ForDownToExpr(identifier, Ast.ConstantExpr(start), Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Constant>, downtoBody: string, doBody: WidgetBuilder<Constant>) =
            Ast.ForDownToExpr(
                identifier,
                Ast.ConstantExpr(start),
                Ast.ConstantExpr(downtoBody),
                Ast.ConstantExpr(doBody)
            )

        static member ForDownToExpr
            (identifier: string, start: WidgetBuilder<Constant>, downtoBody: string, doBody: string)
            =
            Ast.ForDownToExpr(identifier, start, downtoBody, Ast.Constant(doBody))

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: WidgetBuilder<Expr>, doBody: WidgetBuilder<Constant>) =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), downtoBody, Ast.ConstantExpr(doBody))

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: WidgetBuilder<Expr>, doBody: string)
            =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), downtoBody, Ast.Constant(doBody))

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: WidgetBuilder<Constant>, doBody: WidgetBuilder<Expr>) =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: WidgetBuilder<Constant>, doBody: string)
            =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(downtoBody), Ast.Constant(doBody))

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: string, doBody: WidgetBuilder<Expr>)
            =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(downtoBody), doBody)

        static member ForDownToExpr
            (identifier: string, start: string, downtoBody: string, doBody: WidgetBuilder<Constant>)
            =
            Ast.ForDownToExpr(identifier, Ast.Constant(start), Ast.ConstantExpr(downtoBody), Ast.ConstantExpr(doBody))

        static member ForDownToExpr(identifier: string, start: string, downtoBody: string, doBody: string) =
            Ast.ForDownToExpr(identifier, start, downtoBody, Ast.Constant(doBody))
