namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DotIndexedSet =
    let ObjExpr = Attributes.defineWidget "SingleNode"

    let IndexExpr = Attributes.defineWidget "Identifier"

    let ValueExpr = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "DotIndexedSet" (fun widget ->
            let objExpr = Widgets.getNodeFromWidget widget ObjExpr
            let indexExpr = Widgets.getNodeFromWidget widget IndexExpr
            let valueExpr = Widgets.getNodeFromWidget widget ValueExpr
            Expr.DotIndexedSet(ExprDotIndexedSetNode(objExpr, indexExpr, valueExpr, Range.Zero)))

[<AutoOpen>]
module DotIndexedSetBuilders =
    type Ast with

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                DotIndexedSet.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| DotIndexedSet.ObjExpr.WithValue(objExpr.Compile())
                       DotIndexedSet.IndexExpr.WithValue(indexExpr.Compile())
                       DotIndexedSet.ValueExpr.WithValue(valueExpr.Compile()) |],
                    Array.empty
                )
            )

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.DotIndexedSetExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.DotIndexedSetExpr(objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Expr>) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedSetExpr(objExpr, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>, valueExpr: string)
            =
            Ast.DotIndexedSetExpr(objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.DotIndexedSetExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member DotIndexedSetExpr(objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: string) =
            Ast.DotIndexedSetExpr(objExpr, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>, valueExpr: string)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr(objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: string) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Expr>, indexExpr: string, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.DotIndexedSetExpr(objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: string, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>, valueExpr: string)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: WidgetBuilder<Constant>, indexExpr: string, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr, valueExpr)

        static member DotIndexedSetExpr
            (objExpr: string, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member DotIndexedSetExpr(objExpr: string, indexExpr: string, valueExpr: string) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, Ast.ConstantExpr valueExpr)

        static member DotIndexedSetExpr
            (objExpr: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)

        static member DotIndexedSetExpr(objExpr: string, indexExpr: string, valueExpr: WidgetBuilder<Expr>) =
            Ast.DotIndexedSetExpr(Ast.ConstantExpr objExpr, indexExpr, valueExpr)
