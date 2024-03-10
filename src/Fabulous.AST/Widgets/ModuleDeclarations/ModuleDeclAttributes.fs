namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Helpers.getNodeFromWidget<Expr> widget DoExpression

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes


            let multipleAttributes =
                match attributes with
                | Some values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | None -> None

            ModuleDeclAttributesNode(multipleAttributes, doExpression, Range.Zero))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| ModuleDeclAttributes.DoExpression.WithValue(doExpr.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type ModuleDeclAttributeModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ModuleDeclAttributesNode>) =
        AttributeCollectionBuilder<ModuleDeclAttributesNode, AttributeNode>(
            this,
            ModuleDeclAttributes.MultipleAttributes
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ModuleDeclAttributesNode>, attributes: string list) =
        AttributeCollectionBuilder<ModuleDeclAttributesNode, AttributeNode>(
            this,
            ModuleDeclAttributes.MultipleAttributes
        ) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AttributeCollectionBuilder<ModuleDeclAttributesNode, AttributeNode>(
            this,
            ModuleDeclAttributes.MultipleAttributes
        ) {
            attribute
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: string) =
        AttributeCollectionBuilder<ModuleDeclAttributesNode, AttributeNode>(
            this,
            ModuleDeclAttributes.MultipleAttributes
        ) {
            Ast.Attribute(attribute)
        }

[<Extension>]
type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Attributes node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
