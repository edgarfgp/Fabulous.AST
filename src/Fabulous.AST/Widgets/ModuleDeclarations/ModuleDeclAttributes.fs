namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let AttributeNode = Attributes.defineWidget "AttributeListNode"

    let WidgetSingleKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Helpers.getNodeFromWidget<Expr> widget DoExpression
            let attribute = Helpers.tryGetNodeFromWidget<AttributeNode> widget AttributeNode

            let multipleAttributes =
                match attribute with
                | ValueSome values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  [ values ],
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | ValueNone -> None

            ModuleDeclAttributesNode(multipleAttributes, doExpression, Range.Zero))

    let WidgetMultipleKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Helpers.getNodeFromWidget<Expr> widget DoExpression

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            ModuleDeclAttributesNode(multipleAttributes, doExpression, Range.Zero))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        static member ModuleDeclAttributeNode(doExpr: WidgetBuilder<Expr>, attribute: WidgetBuilder<AttributeNode>) =
            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetSingleKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| ModuleDeclAttributes.DoExpression.WithValue(doExpr.Compile())
                           ModuleDeclAttributes.AttributeNode.WithValue(attribute.Compile()) |],
                    ValueNone
                )
            )

        static member ModuleDeclAttributeNode
            (
                doExpr: WidgetBuilder<Expr>,
                attributes: WidgetBuilder<AttributeListNode>
            ) =
            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetMultipleKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| ModuleDeclAttributes.DoExpression.WithValue(doExpr.Compile())
                           ModuleDeclAttributes.MultipleAttributes.WithValue(attributes.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ModuleDeclAttributesNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.Attributes node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
