namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NestedIndexWithoutDot =
    let IdentifierExpr = Attributes.defineWidget "SingleNode"

    let IndexExpr = Attributes.defineWidget "Identifier"

    let ArgumentExpr = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "NestedIndexWithoutDot" (fun widget ->
            let identifierExpr = Widgets.getNodeFromWidget widget IdentifierExpr
            let indexExpr = Widgets.getNodeFromWidget widget IndexExpr
            let argumentExpr = Widgets.getNodeFromWidget widget ArgumentExpr

            Expr.NestedIndexWithoutDot(
                ExprNestedIndexWithoutDotNode(identifierExpr, indexExpr, argumentExpr, Range.Zero)
            ))

[<AutoOpen>]
module NestedIndexWithoutDotBuilders =
    type Ast with

        static member NestedIndexWithoutDotExpr
            (identifierExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, argumentExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                NestedIndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| NestedIndexWithoutDot.IdentifierExpr.WithValue(identifierExpr.Compile())
                       NestedIndexWithoutDot.IndexExpr.WithValue(indexExpr.Compile())
                       NestedIndexWithoutDot.ArgumentExpr.WithValue(argumentExpr.Compile()) |],
                    Array.empty
                )
            )

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NestedIndexWithoutDotExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NestedIndexWithoutDotExpr(objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Expr>) =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>) =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>) =
            Ast.NestedIndexWithoutDotExpr(objExpr, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>) =
            Ast.NestedIndexWithoutDotExpr(
                Ast.ConstantExpr objExpr,
                Ast.ConstantExpr indexExpr,
                Ast.ConstantExpr valueExpr
            )

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, valueExpr: string)
            =
            Ast.NestedIndexWithoutDotExpr(objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NestedIndexWithoutDotExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr(objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: string) =
            Ast.NestedIndexWithoutDotExpr(objExpr, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: string)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr(objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: string) =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NestedIndexWithoutDotExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: string, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: string)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: string, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: string, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr(objExpr: string, indexExpr: string, valueExpr: string) =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member NestedIndexWithoutDotExpr
            (objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member NestedIndexWithoutDotExpr(objExpr: string, indexExpr: string, valueExpr: WidgetBuilder<Expr>) =
            Ast.NestedIndexWithoutDotExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)
