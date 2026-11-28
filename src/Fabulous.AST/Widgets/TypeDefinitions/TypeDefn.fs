namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fantomas.Core.SyntaxOak

/// Common attributes shared by all TypeDefn widgets
module TypeDefn =
    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "TypeDefn_MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "TypeDefn_XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeDefn_TypeParams"

    /// Constraints written after the type name (`type 'T Box when 'T: equality = ...`).
    let Constraints =
        Attributes.defineScalar<TypeConstraint list> "TypeDefn_Constraints"

    let constraints(widget: Widget) =
        Widgets.tryGetScalarValue widget Constraints |> ValueOption.defaultValue []

    let Accessibility = Attributes.defineScalar<AccessControl> "TypeDefn_Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "TypeDefn_IsRecursive"

    let Members = Attributes.defineWidgetCollection "TypeDefn_Members"

    /// Accessibility of a record's fields or a union's cases (`type R = private { ... }`),
    /// as opposed to `Accessibility`, which applies to the type name (`type private R = ...`).
    let RepresentationAccessibility =
        Attributes.defineScalar<AccessControl> "TypeDefn_RepresentationAccessibility"

    let private accessNode(access: AccessControl voption) =
        match access with
        | ValueSome Public -> Some(SingleTextNode.``public``)
        | ValueSome Private -> Some(SingleTextNode.``private``)
        | ValueSome Internal -> Some(SingleTextNode.``internal``)
        | ValueSome Unknown
        | ValueNone -> None

    /// The accessibility set with `toPrivate()`/`toInternal()`/`toPublic()`, printed after `type`.
    let accessibility(widget: Widget) =
        Widgets.tryGetScalarValue widget Accessibility |> accessNode

    /// The accessibility set with `toPrivateRepresentation()` and friends, printed after `=`.
    let representationAccessibility(widget: Widget) =
        Widgets.tryGetScalarValue widget RepresentationAccessibility |> accessNode

/// Modifiers for all TypeDefn widgets
type TypeDefnModifiers =
    /// <summary>Sets the members for the current type definition.</summary>
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefn>) =
        AttributeCollectionBuilder<TypeDefn, MemberDefn>(this, TypeDefn.Members)

    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefn>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeDefn.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefn>, xmlDocs: string seq) =
        TypeDefnModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current type definition.</summary>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefn>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(
            TypeDefn.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attribute for the current type definition.</summary>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefn>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type parameters for the current type definition.</summary>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefn>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefn.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>
    /// Adds <c>when</c> constraints after the type name (<c>type 'T Box when 'T: equality = ...</c>).
    /// This is the only place to constrain prefix type parameters (<c>PrefixList</c>); with
    /// <c>PostfixList</c> the constraints usually go inside the angle brackets instead. Enums,
    /// units of measure and augmentations ignore it.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="constraints">The constraints.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Box") { Field("Value", "'T") }
    ///         |> _.typeParams(PrefixList("'T"))
    ///         |> _.constraints([ ConstraintSingle("'T", "equality") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline constraints(this: WidgetBuilder<TypeDefn>, constraints: WidgetBuilder<TypeConstraint> seq) =
        this.AddScalar(TypeDefn.Constraints.WithValue(constraints |> Seq.map Gen.mkOak |> List.ofSeq))

    /// <summary>Adds a <c>when</c> constraint after the type name (<c>type 'T Box when 'T: equality = ...</c>).</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The constraint.</param>
    [<Extension>]
    static member inline constraints(this: WidgetBuilder<TypeDefn>, value: WidgetBuilder<TypeConstraint>) =
        TypeDefnModifiers.constraints(this, [ value ])

    /// <summary>Sets the type definition to be private.</summary>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the type definition to be public.</summary>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the type definition to be internal.</summary>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>
    /// Makes the fields of a record or the cases of a union private (<c>type R = private { ... }</c>):
    /// the type stays visible but can only be constructed and deconstructed inside its module.
    /// Use <c>toPrivate()</c> to make the type itself private. Other type definitions ignore it.
    /// </summary>
    [<Extension>]
    static member inline toPrivateRepresentation(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.RepresentationAccessibility.WithValue(AccessControl.Private))

    /// <summary>
    /// Makes the fields of a record or the cases of a union internal (<c>type R = internal { ... }</c>).
    /// Use <c>toInternal()</c> to make the type itself internal. Other type definitions ignore it.
    /// </summary>
    [<Extension>]
    static member inline toInternalRepresentation(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.RepresentationAccessibility.WithValue(AccessControl.Internal))

    /// <summary>
    /// Marks the fields of a record or the cases of a union public (<c>type R = public { ... }</c>).
    /// Other type definitions ignore it.
    /// </summary>
    [<Extension>]
    static member inline toPublicRepresentation(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.RepresentationAccessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the type definition to be recursive.</summary>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.IsRecursive.WithValue(true))
