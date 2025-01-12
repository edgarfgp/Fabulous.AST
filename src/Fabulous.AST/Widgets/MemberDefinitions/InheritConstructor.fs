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
        /// <summary>
        /// Create an inherit constructor with the given type.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///        InheritRecordExpr(InheritType(LongIdent("BaseType")))
        ///     }
        /// }
        /// </code>
        static member InheritType(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetTypedOnlyKey,
                InheritConstructor.TypeValue.WithValue(value.Compile())
            )

        /// <summary>
        /// Create an inherit constructor with the given type.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritType("BaseType"))
        ///     }
        /// }
        /// </code>
        static member InheritType(value: string) = Ast.InheritType(Ast.LongIdent(value))

        /// <summary>
        /// Create an inherit constructor with the given type.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", Constructor(ParameterPat("name", String()))) {
        ///             InheritUnit(LongIdent "BaseClass")
        ///     }
        /// }
        /// </code>
        static member InheritUnit(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetUnitKey,
                InheritConstructor.TypeValue.WithValue(value.Compile())
            )

        /// <summary>
        /// Create an inherit constructor with the given type.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", Constructor(ParameterPat("name", String()))) {
        ///             InheritUnit("BaseClass")
        ///         }
        ///     }
        /// }
        /// </code>
        static member InheritUnit(value: string) = Ast.InheritUnit(Ast.LongIdent(value))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen(LongIdent("BaseType "), ConstantExpr(String("123"))))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.empty(),
                    [| InheritConstructor.TypeValue.WithValue(value.Compile())
                       InheritConstructor.ExprValue.WithValue(Ast.ParenExpr(expr).Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen(LongIdent "BaseType ", String("123")))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Constant>) =
            Ast.InheritParen(value, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen(LongIdent "BaseType ", "123"))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: WidgetBuilder<Type>, expr: string) =
            Ast.InheritParen(value, Ast.Constant(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen("BaseType ", ConstantExpr(String("123"))))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InheritParen(Ast.LongIdent(value), expr)

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen("BaseType ", String("123")))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InheritParen(Ast.LongIdent(value), Ast.ConstantExpr(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritParen("BaseType ", "123"))
        ///     }
        /// }
        /// </code>
        static member InheritParen(value: string, expr: string) =
            Ast.InheritParen(value, Ast.Constant(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther(LongIdent("BaseType "), ConstantExpr(String("123"))))
        ///     }
        /// }
        /// </code>
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

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther("BaseType", ConstantExpr(String("123"))))
        ///     }
        /// }
        /// </code>
        static member InheritOther(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InheritOther(Ast.LongIdent(value), expr)

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther("BaseType", String("123")))
        ///     }
        /// }
        /// </code>
        static member InheritOther(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InheritOther(value, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther(LongIdent("BaseType "), String("123")))
        ///     }
        /// }
        /// </code>
        static member InheritOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Constant>) =
            Ast.InheritOther(value, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther(LongIdent("BaseType "), "123"))
        ///     }
        /// }
        /// </code>
        static member InheritOther(value: WidgetBuilder<Type>, expr: string) =
            Ast.InheritOther(value, Ast.Constant(expr))

        /// <summary>
        /// Create an inherit constructor with the given type and expression.
        /// </summary>
        /// <param name="value">The type of the inherit constructor.</param>
        /// <param name="expr">The expression of the inherit constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InheritRecordExpr(InheritOther("BaseType ", "123"))
        ///     }
        /// }
        /// </code>
        static member InheritOther(value: string, expr: string) =
            Ast.InheritOther(Ast.LongIdent(value), expr)
