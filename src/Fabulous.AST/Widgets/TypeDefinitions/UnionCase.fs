namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList


module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let Fields = Attributes.defineWidgetCollection "Fields"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"


    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let fields = Helpers.tryGetNodesFromWidgetCollection<FieldNode> widget Fields

            let fields =
                match fields with
                | Some fields -> fields
                | None -> []

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            UnionCaseNode(None, multipleAttributes, None, SingleTextNode.Create(name), fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Ast with
        static member UnionCase(name: string) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )

        static member UnionParamsCase(name: string) =
            CollectionBuilder<UnionCaseNode, FieldNode>(
                UnionCase.WidgetKey,
                UnionCase.Fields,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )


[<Extension>]
type UnionCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: string list) =
        this.AddScalar(UnionCase.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: WidgetBuilder<AttributeListNode>) =
        this.AddWidget(UnionCase.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        this.AddWidget(UnionCase.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))
