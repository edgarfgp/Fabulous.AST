namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrList =
    let Items = Attributes.defineScalar<Expr list> "Items"

    let OpeningNode = Attributes.defineScalar<SingleTextNode> "OpeningNode"

    let ClosingNode = Attributes.defineScalar<SingleTextNode> "ClosingNode"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let values = Widgets.getScalarValue widget Items
            let openNode = Widgets.getScalarValue widget OpeningNode
            let closeNode = Widgets.getScalarValue widget ClosingNode
            Expr.ArrayOrList(ExprArrayOrListNode(openNode, values, closeNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListBuilders =
    type Ast with
        /// <summary>
        /// Creates a list expression from a list of expression widgets.
        /// </summary>
        /// <param name="value">The list of expression widgets to include in the list.</param>
        /// <returns>A widget builder for a list expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr([ConstantExpr(Int(1)); ConstantExpr(Int(2)); ConstantExpr(Int(3))])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                ArrayOrList.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ArrayOrList.Items.WithValue(parameters),
                        ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftBracket),
                        ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightBracket)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>
        /// Creates a list expression from a list of constant widgets.
        /// </summary>
        /// <param name="value">The list of constant widgets to include in the list.</param>
        /// <returns>A widget builder for a list expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr([Int(1); Int(2); Int(3)])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: WidgetBuilder<Constant> list) =
            let values = value |> List.map Ast.ConstantExpr
            Ast.ListExpr(values)

        /// <summary>
        /// Creates a list expression from a list of string literals.
        /// </summary>
        /// <param name="value">The list of string literals to include in the list.</param>
        /// <returns>A widget builder for a list expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr(["1"; "2"; "3"])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: string list) =
            let values = value |> List.map Ast.Constant
            Ast.ListExpr(values |> List.map Ast.ConstantExpr)

        /// <summary>
        /// Creates an empty list expression.
        /// </summary>
        /// <returns>A widget builder for a list expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         EmptyListExpr()
        ///     }
        /// }
        /// </code>
        static member EmptyListExpr() = Ast.ListExpr(List.empty)

        /// <summary>
        /// Creates an array expression from a list of expression widgets.
        /// </summary>
        /// <param name="value">The list of expression widgets to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr([ConstantExpr(Int(1)); ConstantExpr(Int(2)); ConstantExpr(Int(3))])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                ArrayOrList.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ArrayOrList.Items.WithValue(parameters),
                        ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftArray),
                        ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightArray)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>
        /// Creates an array expression from a list of constant widgets.
        /// </summary>
        /// <param name="value">The list of constant widgets to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr([Int(1); Int(2); Int(3)])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: WidgetBuilder<Constant> list) =
            let values = value |> List.map Ast.ConstantExpr
            Ast.ArrayExpr(values)

        /// <summary>
        /// Creates an array expression from a list of string literals.
        /// </summary>
        /// <param name="value">The list of string literals to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr(["1"; "2"; "3"])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: string list) =
            let values = value |> List.map Ast.Constant
            Ast.ArrayExpr(values |> List.map Ast.ConstantExpr)

        /// <summary>
        /// Creates an empty array expression.
        /// </summary>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         EmptyArrayExpr()
        ///     }
        /// }
        /// </code>
        static member EmptyArrayExpr() = Ast.ArrayExpr(List.empty)
