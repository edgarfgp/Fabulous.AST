namespace Fabulous.AST

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
        static member InheritConstructorTypeOnly(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetTypedOnlyKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member InheritConstructorTypeOnly(value: string) =
            Ast.InheritConstructorTypeOnly(Ast.LongIdent(value))

        static member InheritConstructorUnit(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetUnitKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member InheritConstructorUnit(value: string) =
            Ast.InheritConstructorUnit(Ast.LongIdent(value))

        static member InheritConstructorParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile())
                       InheritConstructor.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile())
                       InheritConstructor.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InheritConstructorOther(Ast.LongIdent(value), expr)

        static member InheritConstructorOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Constant>) =
            Ast.InheritConstructorOther(value, Ast.ConstantExpr(expr))

        static member InheritConstructorOther(value: WidgetBuilder<Type>, expr: string) =
            Ast.InheritConstructorOther(value, Ast.Constant(expr))

        static member InheritConstructorOther(value: string, expr: string) =
            Ast.InheritConstructorOther(Ast.LongIdent(value), expr)
