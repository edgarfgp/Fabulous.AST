namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExceptionDefn =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let UnionCase = Attributes.defineWidget "UnionCase"

    let WithKeyword = Attributes.defineScalar<bool> "WithKeyword"

    let Members = Attributes.defineWidgetCollection "MemberDefs"

    let WidgetKey =
        Widgets.register "ExceptionDefn" (fun widget ->

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let unionCase = Widgets.getNodeFromWidget<UnionCaseNode> widget UnionCase

            let memberDefs =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            let withKeyword =
                if not memberDefs.IsEmpty then
                    Some(SingleTextNode.``with``)
                else
                    None

            ExceptionDefnNode(xmlDocs, attributes, accessControl, unionCase, withKeyword, memberDefs, Range.Zero))

[<AutoOpen>]
module ExceptionDefnBuilders =
    type Ast with
        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn(UnionCase("Error", [ Field(String()); Field(Int()) ]))
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: WidgetBuilder<UnionCaseNode>) =
            WidgetBuilder<ExceptionDefnNode>(
                ExceptionDefn.WidgetKey,
                ExceptionDefn.UnionCase.WithValue(value.Compile())
            )

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error")
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string) = Ast.ExceptionDefn(Ast.UnionCase(value))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ Field(String()); Field(Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: WidgetBuilder<FieldNode> list) =
            Ast.ExceptionDefn(Ast.UnionCase(value, parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", Field(String()))
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: WidgetBuilder<FieldNode>) =
            Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ (String()); (Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: WidgetBuilder<Type> list) =
            let parameters = parameters |> List.map Ast.Field
            Ast.ExceptionDefn(Ast.UnionCase(value, parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameter of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", String())
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: WidgetBuilder<Type>) =
            Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ "string"; "int" ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: string list) =
            let parameters = parameters |> List.map Ast.Field
            Ast.ExceptionDefn(Ast.UnionCase(value, parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameter of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", "string")
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: string) = Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ ("a", String()); ("b", Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: (string * WidgetBuilder<Type>) list) =
            Ast.ExceptionDefn(Ast.UnionCase(value, parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ ("a", "string"); ("b", "int") ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: (string * string) list) =
            Ast.ExceptionDefn(Ast.UnionCase(value, parameters))

type ExceptionDefnModifiers =
    /// <summary>Sets the XmlDocs for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .xmlDocs(Summary("This is an exception definition"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExceptionDefnNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ExceptionDefn.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .xmlDocs([ "This is an exception definition" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExceptionDefnNode>, xmlDocs: string list) =
        ExceptionDefnModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the members for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .members() {
    ///                 Member("Message", String("")).toStatic()
    ///             }
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline members(this: WidgetBuilder<ExceptionDefnNode>) =
        AttributeCollectionBuilder<ExceptionDefnNode, MemberDefn>(this, ExceptionDefn.Members)

    /// <summary>Sets the attributes for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .attributes([ Attribute("MyAttribute") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ExceptionDefnNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ExceptionDefn.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .attribute(Attribute("MyAttribute"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExceptionDefnNode>, attribute: WidgetBuilder<AttributeNode>) =
        ExceptionDefnModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the exception definition to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ExceptionDefnNode>) =
        this.AddScalar(ExceptionDefn.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the exception definition to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ExceptionDefnNode>) =
        this.AddScalar(ExceptionDefn.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the exception definition to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ExceptionDefnNode>) =
        this.AddScalar(ExceptionDefn.Accessibility.WithValue(AccessControl.Internal))

type ExceptionDefnNodeYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ExceptionDefnNode) : CollectionContent =
        let moduleDecl = ModuleDecl.Exception x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExceptionDefnNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ExceptionDefnNodeYieldExtensions.Yield(this, node)
