namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST

module ImplicitConstructor =

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let Pattern = Attributes.defineWidget "Pattern"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Alias = Attributes.defineScalar<string> "Alias"

    let WidgetKey =
        Widgets.register "ImplicitConstructor" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let pattern = Widgets.getNodeFromWidget<Pattern> widget Pattern

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let alias =
                match Widgets.tryGetScalarValue widget Alias with
                | ValueNone -> None
                | ValueSome value ->
                    let value = PrettyNaming.NormalizeIdentifierBackticks value
                    Some(AsSelfIdentifierNode(SingleTextNode.``as``, SingleTextNode.Create(value), Range.Zero))

            ImplicitConstructorNode(xmlDocs, attributes, accessControl, pattern, alias, Range.Zero))

[<AutoOpen>]
module ImplicitConstructorBuilders =
    type Ast with
        static member Constructor(pattern: WidgetBuilder<Pattern>) =
            let pattern =
                match Gen.mkOak pattern with
                | Pattern.Paren _ -> pattern
                | Pattern.Unit _ -> pattern
                | _ -> Ast.ParenPat(pattern)

            WidgetBuilder<ImplicitConstructorNode>(
                ImplicitConstructor.WidgetKey,
                ImplicitConstructor.Pattern.WithValue(pattern.Compile())
            )

        static member Constructor(pattern: WidgetBuilder<Constant>) =
            Ast.Constructor(Ast.ConstantPat(pattern))

        static member Constructor(pattern: string) = Ast.Constructor(Ast.Constant(pattern))

        static member Constructor() = Ast.Constructor(Ast.UnitPat())

type ImplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ImplicitConstructorNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ImplicitConstructor.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ImplicitConstructorNode>, xmlDocs: string seq) =
        ImplicitConstructorModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ImplicitConstructorNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(
            ImplicitConstructor.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ImplicitConstructorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ImplicitConstructorModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline alias(this: WidgetBuilder<ImplicitConstructorNode>, alias: string) =
        this.AddScalar(ImplicitConstructor.Alias.WithValue(alias))
