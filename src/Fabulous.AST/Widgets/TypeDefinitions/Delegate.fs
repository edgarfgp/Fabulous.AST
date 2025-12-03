namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Delegate =

    let Name = Attributes.defineScalar<string> "Name"
    let Parameters = Attributes.defineScalar<Type seq> "Parameters"

    let Return = Attributes.defineWidget "ReturnType"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "TypeDefnDelegateNode" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let returnType = Widgets.getNodeFromWidget<Type> widget Return

            let parameters = Widgets.getScalarValue widget Parameters

            let parameters =
                parameters
                |> Seq.mapi(fun i t ->
                    if i = Seq.length parameters - 1 then
                        (t, SingleTextNode.arrow)
                    else
                        (t, SingleTextNode.star))
                |> List.ofSeq

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown
                |> function
                    | Public -> Some(SingleTextNode.``public``)
                    | Private -> Some(SingleTextNode.``private``)
                    | Internal -> Some(SingleTextNode.``internal``)
                    | Unknown -> None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget IsRecursive
                |> ValueOption.map(fun _ -> SingleTextNode.``and``)
                |> ValueOption.defaultValue SingleTextNode.``type``

            TypeDefnDelegateNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    leadingKeyword,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    None,
                    [],
                    None,
                    Some SingleTextNode.equals,
                    None,
                    Range.Zero
                ),
                SingleTextNode.``delegate``,
                TypeFunsNode(parameters, returnType, Range.Zero),
                Range.Zero
            ))

[<AutoOpen>]
module DelegateBuilders =
    type Ast with
        static member private BaseDelegate
            (name: string, parameters: WidgetBuilder<Type> seq, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<TypeDefnDelegateNode>(
                Delegate.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Delegate.Name.WithValue(name),
                        Delegate.Parameters.WithValue(parameters |> Seq.map Gen.mkOak)
                    ),
                    [| Delegate.Return.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ Paren(Tuple([ Int(); Int() ])); Paren(Tuple([ Int(); Int() ])) ], Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: WidgetBuilder<Type> seq, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters, returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ "int" ; "int" ], Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: string seq, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters |> Seq.map Ast.LongIdent, returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ "int" ; "int" ], "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: string seq, returnType: string) =
            Ast.BaseDelegate(name, parameters |> Seq.map Ast.LongIdent, Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ Int(); Int() ], "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: WidgetBuilder<Type> seq, returnType: string) =
            Ast.BaseDelegate(name, parameters, Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", Int(), Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ parameter ], returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", Int(), "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: string) =
            Ast.BaseDelegate(name, [ parameter ], Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", "int", Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ Ast.LongIdent parameter ], returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", "int", "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: string, returnType: string) =
            Ast.BaseDelegate(name, [ Ast.LongIdent parameter ], Ast.LongIdent returnType)

type DelegateModifiers =
    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.xmlDocs(Summary("This is a delegate"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnDelegateNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Delegate.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.xmlDocs([ "This is a delegate" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnDelegateNode>, xmlDocs: string seq) =
        DelegateModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnDelegateNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(
            Delegate.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.attribute(Attribute("Obsolete"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnDelegateNode>, attribute: WidgetBuilder<AttributeNode>) =
        DelegateModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type definition to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnDelegateNode>) =
        this.AddScalar(Delegate.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the type definition to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnDelegateNode>) =
        this.AddScalar(Delegate.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the type definition to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnDelegateNode>) =
        this.AddScalar(Delegate.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the type definition to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Delegate("Delegate", "int", "int")
    ///         |> _.toRecursive()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefnDelegateNode>) =
        this.AddScalar(Delegate.IsRecursive.WithValue(true))

type DelegateYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnDelegateNode) : CollectionContent =
        let typeDefn = TypeDefn.Delegate(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnDelegateNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        DelegateYieldExtensions.Yield(this, node)
