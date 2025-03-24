namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
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

        /// <summary>Creates an attribute with the specified name.</summary>
        /// <param name="value">The name of the attribute.</param>
        static member Attribute(value: string) =
            WidgetBuilder<AttributeNode>(AttributeNode.WidgetKey, AttributeNode.TypeName.WithValue(value))

        /// <summary>Creates an attribute with a specified target.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="target">The target of the attribute.</param>
        static member AttributeTarget(value: string, target: string) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.two(AttributeNode.TypeName.WithValue(value), AttributeNode.Target.WithValue(target)),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>Creates an attribute with the specified name and expression value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The expression value of the attribute.</param>
        static member Attribute(value: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.one(AttributeNode.TypeName.WithValue(value)),
                    [| AttributeNode.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates an attribute with the specified name and constant value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The constant value of the attribute.</param>
        static member Attribute(value: string, expr: WidgetBuilder<Constant>) =
            Ast.Attribute(value, Ast.ConstantExpr(expr))

        /// <summary>Creates an attribute with the specified name and string value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The string value of the attribute.</param>
        static member Attribute(value: string, expr: string) =
            Ast.Attribute(value, Ast.Constant(expr))

        /// <summary>Creates an attribute with a specified target and expression value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The expression value of the attribute.</param>
        /// <param name="target">The target of the attribute.</param>
        static member AttributeTarget(value: string, expr: WidgetBuilder<Expr>, target: string) =
            WidgetBuilder<AttributeNode>(
                AttributeNode.WidgetKey,
                AttributesBundle(
                    StackList.two(AttributeNode.TypeName.WithValue(value), AttributeNode.Target.WithValue(target)),
                    [| AttributeNode.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates an attribute with a specified target and constant value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The constant value of the attribute.</param>
        /// <param name="target">The target of the attribute.</param>
        static member AttributeTarget(value: string, expr: WidgetBuilder<Constant>, target: string) =
            Ast.AttributeTarget(value, Ast.ConstantExpr(expr), target)

        /// <summary>Creates an attribute with a specified target and string value.</summary>
        /// <param name="value">The name of the attribute.</param>
        /// <param name="expr">The string value of the attribute.</param>
        /// <param name="target">The target of the attribute.</param>
        static member AttributeTarget(value: string, expr: string, target: string) =
            Ast.AttributeTarget(value, Ast.Constant(expr), target)
