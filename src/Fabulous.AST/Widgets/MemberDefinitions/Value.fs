namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module BindingValue =

    let Name = Attributes.defineScalar<Pattern> "Name"

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode> "LeadingKeyword"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let leadingKeyword = Widgets.getScalarValue widget LeadingKeyword
            let bodyExpr = Widgets.getNodeFromWidget widget BindingNode.BodyExpr

            let accessControl =
                Widgets.tryGetScalarValue widget BindingNode.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget BindingNode.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget BindingNode.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isMutable =
                Widgets.tryGetScalarValue widget BindingNode.IsMutable
                |> ValueOption.defaultValue false

            let isInlined =
                Widgets.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType =
                Widgets.tryGetNodeFromWidget widget BindingNode.Return
                |> ValueOption.map(fun x -> Some(BindingReturnInfoNode(SingleTextNode.colon, x, Range.Zero)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget BindingNode.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            BindingNode(
                xmlDocs,
                attributes,
                MultipleTextsNode([ leadingKeyword ], Range.Zero),
                isMutable,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice2Of2(name),
                typeParams,
                [],
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingValueBuilders =
    type Ast with
        static member private BaseValue
            (name: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>, leadingKeyword: SingleTextNode)
            =
            WidgetBuilder<BindingNode>(
                BindingValue.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        BindingValue.Name.WithValue(Gen.mkOak name),
                        BindingValue.LeadingKeyword.WithValue(leadingKeyword)
                    ),
                    [| BindingNode.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
        ///     }
        /// }
        /// </code>
        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, SingleTextNode.``let``)

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value(ConstantPat(Constant("x")), Int(12))
        ///     }
        /// }
        /// </code>
        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.Value(name, Ast.ConstantExpr(value))

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value(ConstantPat(Constant("x")), "12")
        ///     }
        /// }
        /// </code>
        static member Value(name: WidgetBuilder<Pattern>, value: string) =
            Ast.Value(name, Ast.ConstantExpr(Ast.Constant(value)))

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value(Constant("x"), Int(12))
        ///     }
        /// }
        /// </code>
        static member Value(name: WidgetBuilder<Constant>, value: WidgetBuilder<Constant>) =
            Ast.Value(Ast.ConstantPat(name), Ast.ConstantExpr(value))

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", ConstantExpr(Int(12)))
        ///     }
        /// }
        /// </code>
        static member Value(name: string, value: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), value)

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", Int(12))
        ///     }
        /// }
        /// </code>
        static member Value(name: string, value: WidgetBuilder<Constant>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), Ast.ConstantExpr(value))

        /// <summary>
        /// Create a binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <param name="value">The value of the binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", "12")
        ///     }
        /// }
        /// </code>
        static member Value(name: string, value: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), Ast.ConstantExpr(Ast.Constant(value)))

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
        ///     }
        /// }
        /// </code>
        static member Use(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, SingleTextNode.``use``)

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use(ConstantPat(Constant("x")), Int(12))
        ///     }
        /// }
        /// </code>
        static member Use(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.BaseValue(name, Ast.ConstantExpr(value), SingleTextNode.``use``)

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use(ConstantPat(Constant("x")), "12")
        ///     }
        /// }
        /// </code>
        static member Use(name: WidgetBuilder<Pattern>, value: string) = Ast.Use(name, Ast.Constant(value))

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use("x", ConstantExpr(Int(12)))
        ///     }
        /// }
        /// </code>
        static member Use(name: string, value: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Use(Ast.ConstantPat(name), value)

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use("x", Int(12))
        ///     }
        /// }
        /// </code>
        static member Use(name: string, value: WidgetBuilder<Constant>) = Ast.Use(name, Ast.ConstantExpr(value))

        /// <summary>
        /// Create a use binding with the given name and value.
        /// </summary>
        /// <param name="name">The name of the use binding.</param>
        /// <param name="value">The value of the use binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Use("x", "12")
        ///     }
        /// }
        /// </code>
        static member Use(name: string, value: string) = Ast.Use(name, Ast.Constant(value))
