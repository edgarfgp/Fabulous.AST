namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Widgets.getNodeFromWidget<Expr> widget DoExpression

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
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
                | ValueNone -> None

            ModuleDeclAttributesNode(multipleAttributes, doExpression, Range.Zero))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ModuleDeclAttributes.DoExpression.WithValue(doExpr.Compile()) |],
                    Array.empty
                )
            )

type ModuleDeclAttributeModifiers =
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ModuleDeclAttributes.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ModuleDeclAttributesNode>, attributes: string list) =
        ModuleDeclAttributeModifiers.attributes(
            this,
            [ for attribute in attributes do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ModuleDeclAttributeModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: string) =
        ModuleDeclAttributeModifiers.attributes(this, [ Ast.Attribute(attribute) ])

type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Attributes node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
