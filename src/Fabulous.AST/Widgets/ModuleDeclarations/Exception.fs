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
        static member ExceptionDefn(value: WidgetBuilder<UnionCaseNode>) =
            WidgetBuilder<ExceptionDefnNode>(
                ExceptionDefn.WidgetKey,
                ExceptionDefn.UnionCase.WithValue(value.Compile())
            )

        static member ExceptionDefn(value: string) = Ast.ExceptionDefn(Ast.UnionCase(value))

        static member ExceptionDefn(name: string, parameters: WidgetBuilder<FieldNode> list) =
            Ast.ExceptionDefn(Ast.UnionCase(name, parameters))

        static member ExceptionDefn(name: string, parameter: WidgetBuilder<FieldNode>) =
            Ast.ExceptionDefn(Ast.UnionCase(name, [ parameter ]))

        static member ExceptionDefn(name: string, parameters: (string * string) list) =
            Ast.ExceptionDefn(Ast.UnionCase(name, parameters))

        static member ExceptionDefn(name: string, parameters: (string * WidgetBuilder<Type>) list) =
            Ast.ExceptionDefn(Ast.UnionCase(name, parameters))

type ExceptionDefnModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExceptionDefnNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ExceptionDefn.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExceptionDefnNode>, xmlDocs: string list) =
        ExceptionDefnModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline members(this: WidgetBuilder<ExceptionDefnNode>) =
        AttributeCollectionBuilder<ExceptionDefnNode, MemberDefn>(this, ExceptionDefn.Members)

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

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExceptionDefnNode>, attribute: WidgetBuilder<AttributeNode>) =
        ExceptionDefnModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ExceptionDefnNode>) =
        this.AddScalar(ExceptionDefn.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ExceptionDefnNode>) =
        this.AddScalar(ExceptionDefn.Accessibility.WithValue(AccessControl.Public))

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
