namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExplicitConstructorMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Pat = Attributes.defineWidget "Pat"

    let Alias = Attributes.defineScalar<string> "Alias"

    let ExprValue = Attributes.defineWidget "ExprValue"

    let WidgetKey =
        Widgets.register "ExplicitConstructorMember" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget MemberDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let pat = Widgets.getNodeFromWidget widget Pat

            let alias =
                match Widgets.tryGetScalarValue widget Alias with
                | ValueSome value -> Some(SingleTextNode.Create value)
                | ValueNone -> None

            let expr = Widgets.getNodeFromWidget widget ExprValue

            let node =
                MemberDefnExplicitCtorNode(
                    xmlDocs,
                    attributes,
                    accessControl,
                    SingleTextNode.``new``,
                    pat,
                    alias,
                    SingleTextNode.equals,
                    expr,
                    Range.Zero
                )

            MemberDefn.ExplicitCtor(node))

[<AutoOpen>]
module ExplicitConstructorBuilders =
    type Ast with

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///        TypeDefn("MyClass", UnitPat()) {
        ///            Constructor(
        ///                UnitPat(),
        ///                AppSingleParenArgExpr(
        ///                    "MyClass",
        ///                    ParenExpr(TupleExpr([ Int(0); Int(0); Int(0)]))
        ///                )
        ///            )
        ///       }
        /// </code>
        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            let pattern =
                match Gen.mkOak pattern with
                | Pattern.Unit _
                | Pattern.Paren _ -> pattern
                | _ -> Ast.ParenPat(pattern)

            WidgetBuilder<MemberDefn>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ExplicitConstructorMember.Pat.WithValue(pattern.Compile())
                       ExplicitConstructorMember.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 UnitPat(),
        ///                 AppSingleParenArgExpr(
        ///                     "MyClass",
        ///                     ParenExpr(TupleExpr([ Int(0); Int(0); Int(0)]))
        ///                 )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Constructor(Ast.ConstantPat(pattern), expr)

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 "(x)",
        ///                 AppSingleParenArgExpr(
        ///                     "MyClass",
        ///                     ParenExpr(TupleExpr([ Int(0); Int(0); Int(0)]))
        ///                 )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: string, expr: WidgetBuilder<Expr>) =
            Ast.Constructor(Ast.Constant(pattern), expr)

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 UnitPat(),
        ///                 Constant("MyClass(0, 0, 0)")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Constructor(pattern, Ast.ConstantExpr(expr))

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 Constant("x"),
        ///                 Constant("MyClass(0, 0, 0)")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.Constructor(Ast.ConstantPat(pattern), Ast.ConstantExpr(expr))

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 UnitPat(),
        ///                 "MyClass(0, 0, 0)"
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: string) =
            Ast.Constructor(pattern, Ast.Constant(expr))

        /// <summary>
        /// Creates an explicit constructor with the given pattern and expression.
        /// </summary>
        /// <param name="pattern">The pattern of the constructor.</param>
        /// <param name="expr">The expression of the constructor.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("MyClass", UnitPat()) {
        ///             Constructor(
        ///                 "(x)",
        ///                 "MyClass(x, 0, 0)"
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Constructor(pattern: string, expr: string) =
            Ast.Constructor(Ast.ConstantPat(pattern), expr)

type ExplicitConstructorModifiers =
    /// <summary>Sets the alias for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="alias">The alias to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("MyClass", UnitPat()) {
    ///             Constructor(
    ///                 UnitPat(),
    ///                 Constant("MyClass(0, 0, 0)")
    ///             )
    ///                 .alias("this")
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline alias(this: WidgetBuilder<MemberDefn>, alias: string) =
        this.AddScalar(ExplicitConstructorMember.Alias.WithValue(alias))
