namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Widgets.getNodeFromWidget<Expr> widget DoExpression

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            ModuleDeclAttributesNode(attributes, doExpression, Range.Zero))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetKey,
                ModuleDeclAttributes.DoExpression.WithValue(Ast.SingleExpr(Ast.DoExpr doExpr).Compile())
            )

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(Constant(" printfn \"Executing...\""))
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Constant>) =
            Ast.ModuleDeclAttribute(Ast.ConstantExpr(doExpr))

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(" printfn \"Executing...\"")
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: string) =
            Ast.ModuleDeclAttribute(Ast.Constant(doExpr))

type ModuleDeclAttributeModifiers =
    /// <summary>Sets the attributes for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
    ///             .attributes([ Attribute("MyCustomModuleAttribute") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(ModuleDeclAttributes.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attributes for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
    ///             .attribute(Attribute("MyCustomModuleAttribute"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ModuleDeclAttributeModifiers.attributes(this, [ attribute ])

type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ModuleDeclAttributesNode)
        : CollectionContent =
        let moduleDecl = ModuleDecl.Attributes x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributesYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ModuleDeclAttributesNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.Attributes node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclAttributesYieldExtensions.YieldFrom(this, nodes)
