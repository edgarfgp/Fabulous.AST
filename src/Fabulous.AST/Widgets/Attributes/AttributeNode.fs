namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AttributeNode =
    let TypeName = Attributes.defineScalar<IdentListNode> "TypeName"

    let Expr = Attributes.defineWidget "Value"

    let Target = Attributes.defineScalar<string> "Target"

    let WidgetKey =
        Widgets.register "AttributeNode" (fun widget ->
            let expr = Widgets.tryGetNodeFromWidget<Expr> widget Expr
            let target = Widgets.tryGetScalarValue widget Target

            let expr =
                match expr with
                | ValueNone -> None
                | ValueSome expr -> Some expr

            let target =
                match target with
                | ValueNone -> None
                | ValueSome target -> Some(SingleTextNode.Create(target))

            let typeName = Widgets.getScalarValue widget TypeName
            AttributeNode(typeName, expr, target, Range.Zero))

[<AutoOpen>]
module AttributeNodeBuilders =
    type Ast with
        static member private BaseAttributeNode
            (typeName: string, expr: WidgetBuilder<Expr> voption, target: string voption)
            =
            let widgets =
                match expr with
                | ValueNone -> ValueNone
                | ValueSome expr -> ValueSome [| AttributeNode.Expr.WithValue(expr.Compile()) |]

            let scalars =
                match target with
                | ValueNone ->
                    StackList.one(
                        AttributeNode.TypeName.WithValue(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create typeName) ], Range.Zero)
                        )
                    )
                | ValueSome target ->
                    StackList.two(
                        AttributeNode.TypeName.WithValue(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create typeName) ], Range.Zero)
                        ),
                        AttributeNode.Target.WithValue(target)
                    )

            WidgetBuilder<AttributeNode>(AttributeNode.WidgetKey, AttributesBundle(scalars, widgets, ValueNone))

        static member Attribute(value: string) =
            Ast.BaseAttributeNode(value, ValueNone, ValueNone)

        static member Attribute(value: string, target: string) =
            Ast.BaseAttributeNode(value, ValueNone, ValueSome target)

        static member Attribute(value: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseAttributeNode(value, ValueSome expr, ValueNone)

        static member Attribute(value: string, expr: WidgetBuilder<Expr>, target: string) =
            Ast.BaseAttributeNode(value, ValueSome expr, ValueSome target)
