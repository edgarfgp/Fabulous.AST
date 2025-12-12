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

    let Accessibility = Attributes.defineScalar<AccessControl> "TypeDefn_Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "TypeDefn_IsRecursive"

    let Members = Attributes.defineWidgetCollection "TypeDefn_Members"

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

    /// <summary>Sets the type definition to be recursive.</summary>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefn>) =
        this.AddScalar(TypeDefn.IsRecursive.WithValue(true))
