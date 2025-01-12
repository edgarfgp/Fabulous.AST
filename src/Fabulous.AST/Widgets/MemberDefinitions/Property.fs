namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module BindingProperty =
    let Name = Attributes.defineScalar<Pattern> "Name"

    let WidgetKey =
        Widgets.register "PropertyMember" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let bodyExpr = Widgets.getNodeFromWidget widget BindingNode.BodyExpr
            let isInlined = Widgets.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType =
                Widgets.tryGetNodeFromWidget widget BindingNode.Return
                |> ValueOption.map(fun value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero)))
                |> ValueOption.defaultValue None

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

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false
                | ValueNone -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget BindingNode.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                [ if isStatic then
                      SingleTextNode.``static``
                      SingleTextNode.``member``
                  else
                      SingleTextNode.``member`` ]

            BindingNode(
                xmlDocs,
                attributes,
                MultipleTextsNode(multipleTextsNode, Range.Zero),
                false,
                inlineNode,
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
module BindingPropertyBuilders =
    type Ast with
        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///        TypeDefn("Person", UnitPat()) {
        ///            Member(ConstantPat(Constant("this.Name")), ConstantExpr(Int 23))
        ///        }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>) =
            WidgetBuilder<BindingNode>(
                BindingProperty.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingProperty.Name.WithValue(Gen.mkOak name)),
                    [| BindingNode.BodyExpr.WithValue(body.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(ConstantPat("this.Name"), Int(23))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Pattern>, body: WidgetBuilder<Constant>) =
            Ast.Member(name, Ast.ConstantExpr(body))

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(ConstantPat("this.Name"), "23")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Pattern>, body: string) =
            Ast.Member(name, Ast.ConstantExpr(Ast.Constant(body)))

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(Constant("this.Name"), ConstantExpr(Int 23))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Constant>, body: WidgetBuilder<Expr>) =
            Ast.Member(Ast.ConstantPat(name), body)

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(Constant("this.Name"), Int(23))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Constant>, body: WidgetBuilder<Constant>) =
            Ast.Member(name, Ast.ConstantExpr(body))

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member("this.Name", ConstantExpr(Int 23))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, body: WidgetBuilder<Expr>) = Ast.Member(Ast.Constant(name), body)

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(Constant("this.Name"), "23")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: WidgetBuilder<Constant>, body: string) =
            Ast.Member(name, Ast.ConstantExpr(Ast.Constant(body)))

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member("this.Name", Int(23))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, body: WidgetBuilder<Constant>) =
            Ast.Member(name, Ast.ConstantExpr(body))

        /// <summary>
        /// Create a member property with the given name and body.
        /// </summary>
        /// <param name="name">The name of the member property.</param>
        /// <param name="body">The body of the member property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member("this.Name", "23")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, body: string) =
            Ast.Member(Ast.Constant(name), Ast.Constant(body))
