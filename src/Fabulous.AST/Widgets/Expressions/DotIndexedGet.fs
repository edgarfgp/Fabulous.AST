namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DotIndexedGet =
    let ObjExpr = Attributes.defineWidget "SingleNode"

    let IndexExpr = Attributes.defineWidget "Identifier"

    let WidgetKey =
        Widgets.register "DotIndexedGet" (fun widget ->
            let objExpr = Widgets.getNodeFromWidget widget ObjExpr
            let indexExpr = Widgets.getNodeFromWidget widget IndexExpr
            Expr.DotIndexedGet(ExprDotIndexedGetNode(objExpr, indexExpr, Range.Zero)))

[<AutoOpen>]
module DotIndexedGetBuilders =
    type Ast with

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                DotIndexedGet.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| DotIndexedGet.ObjExpr.WithValue(objExpr.Compile())
                       DotIndexedGet.IndexExpr.WithValue(indexExpr.Compile()) |],
                    Array.empty
                )
            )

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Expr>) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr objExpr, indexExpr)

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Expr>, indexExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedGetExpr(objExpr, Ast.ConstantExpr indexExpr)

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Constant>, indexExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr indexExpr)

        static member DotIndexedGetExpr(objExpr: string, indexExpr: WidgetBuilder<Constant>) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr(objExpr), Ast.ConstantExpr indexExpr)

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Constant>, indexExpr: string) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr objExpr, Ast.ConstantExpr(indexExpr))

        static member DotIndexedGetExpr(objExpr: string, indexExpr: string) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr(objExpr), Ast.ConstantExpr(indexExpr))

        static member DotIndexedGetExpr(objExpr: WidgetBuilder<Expr>, indexExpr: string) =
            Ast.DotIndexedGetExpr(objExpr, Ast.ConstantExpr(indexExpr))

        static member DotIndexedGetExpr(objExpr: string, indexExpr: WidgetBuilder<Expr>) =
            Ast.DotIndexedGetExpr(Ast.ConstantExpr(objExpr), indexExpr)
