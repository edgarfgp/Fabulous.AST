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

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            ModuleDeclAttributesNode(attributes, doExpression, Range.Zero))

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

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Constant>) =
            Ast.ModuleDeclAttribute(Ast.ConstantExpr(doExpr))

        static member ModuleDeclAttribute(doExpr: string) =
            Ast.ModuleDeclAttribute(Ast.Constant(doExpr))

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
    static member inline attribute
        (this: WidgetBuilder<ModuleDeclAttributesNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ModuleDeclAttributeModifiers.attributes(this, [ attribute ])

type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Attributes node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
