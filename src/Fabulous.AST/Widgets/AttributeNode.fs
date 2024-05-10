namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module AttributeNode =
    let TypeName = Attributes.defineScalar<string> "TypeName"

    let Value = Attributes.defineWidget "Value"

    let Target = Attributes.defineScalar<string> "Target"

    let WidgetKey =
        Widgets.register "AttributeNode" (fun widget ->
            let expr =
                Widgets.tryGetNodeFromWidget widget Value
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let target =
                Widgets.tryGetScalarValue widget Target
                |> ValueOption.map(fun target -> Some(SingleTextNode.Create(target)))
                |> ValueOption.defaultValue None

            let typeName = Widgets.getScalarValue widget TypeName

            AttributeNode(
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create typeName) ], Range.Zero),
                expr,
                target,
                Range.Zero
            ))

[<AutoOpen>]
module AttributeNodeBuilders =
    type Ast with

        static member Attribute(value: string) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(StackList.one(AttributeNode.TypeName.WithValue(value)), Array.empty, Array.empty)
            )

        static member AttributeTarget(value: string, target: string) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.two(AttributeNode.TypeName.WithValue(value), AttributeNode.Target.WithValue(target)),
                    Array.empty,
                    Array.empty
                )
            )

        static member Attribute(value: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.one(AttributeNode.TypeName.WithValue(value)),
                    [| AttributeNode.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Attribute(value: string, expr: WidgetBuilder<Constant>) =
            Ast.Attribute(value, Ast.ConstantExpr(expr))

        static member Attribute(value: string, expr: string) =
            Ast.Attribute(value, Ast.Constant(expr))

        static member AttributeTarget(value: string, expr: WidgetBuilder<Expr>, target: string) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.two(AttributeNode.TypeName.WithValue(value), AttributeNode.Target.WithValue(target)),
                    [| AttributeNode.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member AttributeTarget(value: string, expr: WidgetBuilder<Constant>, target: string) =
            Ast.AttributeTarget(value, Ast.ConstantExpr(expr), target)

        static member AttributeTarget(value: string, expr: string, target: string) =
            Ast.AttributeTarget(value, Ast.Constant(expr), target)
