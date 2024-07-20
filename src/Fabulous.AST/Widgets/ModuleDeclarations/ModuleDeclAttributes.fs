namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"

    let Attributes = Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Widgets.getNodeFromWidget<Expr> widget DoExpression

            let attributes =
                Widgets.tryGetScalarValue widget Attributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            ModuleDeclAttributesNode(attributes, doExpression, Range.Zero))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>, attributes: WidgetBuilder<AttributeNode> list) =
            let attributes = attributes |> List.map(Gen.mkOak)

            WidgetBuilder<ModuleDeclAttributesNode>(
                ModuleDeclAttributes.WidgetKey,
                AttributesBundle(
                    StackList.one(ModuleDeclAttributes.Attributes.WithValue(attributes)),
                    [| ModuleDeclAttributes.DoExpression.WithValue(doExpr.Compile()) |],
                    Array.empty
                )
            )

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>, attribute: WidgetBuilder<AttributeNode>) =
            Ast.ModuleDeclAttribute(doExpr, [ attribute ])

        static member ModuleDeclAttribute
            (doExpr: WidgetBuilder<Constant>, attributes: WidgetBuilder<AttributeNode> list)
            =
            Ast.ModuleDeclAttribute(Ast.ConstantExpr(doExpr), attributes)

        static member ModuleDeclAttribute(doExpr: string, attributes: WidgetBuilder<AttributeNode> list) =
            Ast.ModuleDeclAttribute(Ast.Constant(doExpr), attributes)

        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Constant>, attribute: WidgetBuilder<AttributeNode>) =
            Ast.ModuleDeclAttribute(Ast.ConstantExpr(doExpr), [ attribute ])

        static member ModuleDeclAttribute(doExpr: string, attribute: WidgetBuilder<AttributeNode>) =
            Ast.ModuleDeclAttribute(Ast.Constant(doExpr), [ attribute ])

type ModuleDeclAttributesYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ModuleDeclAttributesNode)
        : CollectionContent =
        let moduleDecl = ModuleDecl.Attributes x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleDeclAttributesNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributesYieldExtensions.Yield(this, node)
