namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrList =
    let Items = Attributes.defineScalar<Expr seq> "Items"

    let OpeningNode = Attributes.defineScalar<SingleTextNode> "OpeningNode"

    let ClosingNode = Attributes.defineScalar<SingleTextNode> "ClosingNode"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let values = Widgets.getScalarValue widget Items |> List.ofSeq
            let openNode = Widgets.getScalarValue widget OpeningNode
            let closeNode = Widgets.getScalarValue widget ClosingNode
            Expr.ArrayOrList(ExprArrayOrListNode(openNode, values, closeNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListBuilders =
    type Ast with
        /// <summary>
        /// Creates a list expression from a seq of expression widgets.
        /// </summary>
        /// <param name="value">The seq of expression widgets to include in the seq.</param>
        /// <returns>A widget builder for a seq expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr([ConstantExpr(Int(1)); ConstantExpr(Int(2)); ConstantExpr(Int(3))])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: WidgetBuilder<Expr> seq) =
            let parameters = value |> Seq.map Gen.mkOak

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
        /// Creates a seq expression from a seq of constant widgets.
        /// </summary>
        /// <param name="value">The seq of constant widgets to include in the seq.</param>
        /// <returns>A widget builder for a seq expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr([Int(1); Int(2); Int(3)])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: WidgetBuilder<Constant> seq) =
            let values = value |> Seq.map Ast.ConstantExpr
            Ast.ListExpr(values)

        /// <summary>
        /// Creates a seq expression from a seq of string literals.
        /// </summary>
        /// <param name="value">The seq of string literals to include in the seq.</param>
        /// <returns>A widget builder for a seq expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ListExpr(["1"; "2"; "3"])
        ///     }
        /// }
        /// </code>
        static member ListExpr(value: string seq) =
            let values = value |> Seq.map Ast.Constant
            Ast.ListExpr(values |> Seq.map Ast.ConstantExpr)

        /// <summary>
        /// Creates an empty seq expression.
        /// </summary>
        /// <returns>A widget builder for a seq expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         EmptyListExpr()
        ///     }
        /// }
        /// </code>
        static member EmptyListExpr() = Ast.ListExpr(List.empty)

        /// <summary>
        /// Creates an array expression from a seq of expression widgets.
        /// </summary>
        /// <param name="value">The seq of expression widgets to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr([ConstantExpr(Int(1)); ConstantExpr(Int(2)); ConstantExpr(Int(3))])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: WidgetBuilder<Expr> seq) =
            let parameters = value |> Seq.map Gen.mkOak

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
        /// Creates an array expression from a seq of constant widgets.
        /// </summary>
        /// <param name="value">The seq of constant widgets to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr([Int(1); Int(2); Int(3)])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: WidgetBuilder<Constant> seq) =
            let values = value |> Seq.map Ast.ConstantExpr
            Ast.ArrayExpr(values)

        /// <summary>
        /// Creates an array expression from a seq of string literals.
        /// </summary>
        /// <param name="value">The seq of string literals to include in the array.</param>
        /// <returns>A widget builder for an array expression.</returns>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ArrayExpr(["1"; "2"; "3"])
        ///     }
        /// }
        /// </code>
        static member ArrayExpr(value: string seq) =
            let values = value |> Seq.map Ast.Constant
            Ast.ArrayExpr(values |> Seq.map Ast.ConstantExpr)

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
