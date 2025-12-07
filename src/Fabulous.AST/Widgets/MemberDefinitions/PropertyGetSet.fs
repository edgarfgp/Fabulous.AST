namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let FirstBindingWidget = Attributes.defineWidget "GetterWidget"
    let LastBindingWidget = Attributes.defineWidget "SetterWidget"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "PropertyGetSetMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier

            let isInlined =
                Widgets.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let accessControl =
                Widgets.tryGetScalarValue widget MemberDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                MultipleTextsNode(
                    (if isStatic then
                         [ SingleTextNode.``static``; SingleTextNode.``member`` ]
                     else
                         [ SingleTextNode.``member`` ]),
                    Range.Zero
                )

            let inlinedNode = if isInlined then Some SingleTextNode.``inline`` else None

            let memberName =
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero)

            let firstBinding =
                Widgets.tryGetNodeFromWidget<PropertyGetSetBindingNode> widget FirstBindingWidget

            let firstBinding =
                match firstBinding with
                | ValueSome value -> value
                | ValueNone -> failwith "Getter is required"

            let lastBindingWidget =
                Widgets.tryGetNodeFromWidget<PropertyGetSetBindingNode> widget LastBindingWidget
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let andKeyword =
                if lastBindingWidget.IsSome then
                    Some SingleTextNode.``and``
                else
                    None

            let node =
                MemberDefnPropertyGetSetNode(
                    xmlDocs,
                    attributes,
                    multipleTextsNode,
                    inlinedNode,
                    accessControl,
                    memberName,
                    SingleTextNode.``with``,
                    firstBinding,
                    andKeyword,
                    lastBindingWidget,
                    Range.Zero
                )

            MemberDefn.PropertyGetSet(node))

[<AutoOpen>]
module PropertyGetSetMemberMemberBuilders =
    type Ast with

        /// <summary>Create a property with a getter.</summary>
        /// <param name="identifier">The name of the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 Getter(ConstantExpr("_name"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(identifier: string, getter: WidgetBuilder<PropertyGetSetBindingNode>) =
            WidgetBuilder<MemberDefn>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.FirstBindingWidget.WithValue(getter.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a property with a getter and setter.</summary>
        /// <param name="identifier">The name of the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <param name="setter">The setter for the property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 Getter(ConstantExpr("_name")),
        ///                 Setter(NamedPat("value"), ConstantExpr("_name &lt;- value"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member
            (
                identifier: string,
                getter: WidgetBuilder<PropertyGetSetBindingNode>,
                setter: WidgetBuilder<PropertyGetSetBindingNode>
            ) =
            WidgetBuilder<MemberDefn>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.FirstBindingWidget.WithValue(getter.Compile())
                       PropertyGetSetMember.LastBindingWidget.WithValue(setter.Compile()) |],
                    Array.empty
                )
            )
