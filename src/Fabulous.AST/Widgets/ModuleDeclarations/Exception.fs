namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExceptionDefn =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let UnionCase = Attributes.defineWidget "UnionCase"

    let WithKeyword = Attributes.defineScalar<bool> "WithKeyword"

    let Members = Attributes.defineWidgetCollection "MemberDefs"

    let WidgetKey =
        Widgets.register "ExceptionDefn" (fun widget ->
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

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
                AttributesBundle(
                    StackList.empty(),
                    [| ExceptionDefn.UnionCase.WithValue(value.Compile()) |],
                    Array.empty
                )
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
    static member inline xmlDocs(this: WidgetBuilder<ExceptionDefnNode>, comments: string list) =
        this.AddScalar(ExceptionDefn.XmlDocs.WithValue(comments))

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
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExceptionDefnNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Exception node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
