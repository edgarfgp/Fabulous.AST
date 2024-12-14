namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InheritConstructor =
    let TypeValue = Attributes.defineWidget "TypeValue"

    let ExprValue = Attributes.defineWidget "ExprValue"

    let WidgetTypedOnlyKey =
        Widgets.register "TypedOnly" (fun widget ->
            let typeValue = Widgets.getNodeFromWidget widget TypeValue

            InheritConstructor.TypeOnly(
                InheritConstructorTypeOnlyNode(SingleTextNode.``inherit``, typeValue, Range.Zero)
            ))

    let WidgetUnitKey =
        Widgets.register "Unit" (fun widget ->
            let typeValue = Widgets.getNodeFromWidget widget TypeValue

            InheritConstructor.Unit(
                InheritConstructorUnitNode(
                    SingleTextNode.``inherit``,
                    typeValue,
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

    let WidgetParentKey =
        Widgets.register "Paren" (fun widget ->
            let typeValue = Widgets.getNodeFromWidget widget TypeValue
            let expr = Widgets.getNodeFromWidget widget ExprValue

            InheritConstructor.Paren(
                InheritConstructorParenNode(SingleTextNode.``inherit``, typeValue, expr, Range.Zero)
            ))

    let WidgetOtherKey =
        Widgets.register "Other" (fun widget ->
            let typeValue = Widgets.getNodeFromWidget widget TypeValue
            let expr = Widgets.getNodeFromWidget widget ExprValue

            InheritConstructor.Other(
                InheritConstructorOtherNode(SingleTextNode.``inherit``, typeValue, expr, Range.Zero)
            ))

[<AutoOpen>]
module InheritConstructorBuilders =
    type Ast with
        static member InheritType(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetTypedOnlyKey,
                InheritConstructor.TypeValue.WithValue(value.Compile())
            )

        static member InheritType(value: string) = Ast.InheritType(Ast.LongIdent(value))

        static member InheritUnit(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetUnitKey,
                InheritConstructor.TypeValue.WithValue(value.Compile())
            )

        static member InheritUnit(value: string) = Ast.InheritUnit(Ast.LongIdent(value))

        static member InheritParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile())
                       InheritConstructor.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member InheritParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Constant>) =
            Ast.InheritParen(value, Ast.ConstantExpr(expr))

        static member InheritParen(value: WidgetBuilder<Type>, expr: string) =
            Ast.InheritParen(value, Ast.Constant(expr))

        static member InheritParen(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InheritParen(Ast.LongIdent(value), expr)

        static member InheritParen(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InheritParen(Ast.LongIdent(value), Ast.ConstantExpr(expr))

        static member InheritParen(value: string, expr: string) =
            Ast.InheritParen(value, Ast.Constant(expr))

        static member InheritOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile())
                       InheritConstructor.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member InheritOther(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InheritOther(Ast.LongIdent(value), expr)

        static member InheritOther(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InheritOther(value, Ast.ConstantExpr(expr))

        static member InheritOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Constant>) =
            Ast.InheritOther(value, Ast.ConstantExpr(expr))

        static member InheritOther(value: WidgetBuilder<Type>, expr: string) =
            Ast.InheritOther(value, Ast.Constant(expr))

        static member InheritOther(value: string, expr: string) =
            Ast.InheritOther(Ast.LongIdent(value), expr)
