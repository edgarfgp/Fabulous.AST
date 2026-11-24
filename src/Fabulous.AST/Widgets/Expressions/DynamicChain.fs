namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DynamicChainItem =
    let MemberExpr = Attributes.defineWidget "MemberExpr"

    let ArgExpr = Attributes.defineWidget "ArgExpr"

    let WidgetKey =
        Widgets.register "DynamicChainItem" (fun widget ->
            let memberExpr = Widgets.getNodeFromWidget<Expr> widget MemberExpr

            // The argument sits right after the member name (`?name(arg)`), so it must be
            // parenthesized or unit; anything else is wrapped in parentheses.
            let argExpr =
                Widgets.tryGetNodeFromWidget<Expr> widget ArgExpr
                |> ValueOption.toOption
                |> Option.map(fun expr ->
                    match expr with
                    | Expr.Paren _
                    | Expr.Constant(Constant.Unit _) -> expr
                    | _ ->
                        Expr.Paren(
                            ExprParenNode(
                                SingleTextNode.leftParenthesis,
                                expr,
                                SingleTextNode.rightParenthesis,
                                Range.range0
                            )
                        ))

            ExprDynamicChainItemNode(memberExpr, argExpr, Range.range0))

module DynamicChain =
    let LeadingExpr = Attributes.defineWidget "LeadingExpr"

    let Items = Attributes.defineScalar<ExprDynamicChainItemNode list> "Items"

    let WidgetKey =
        Widgets.register "DynamicChain" (fun widget ->
            let leadingExpr = Widgets.getNodeFromWidget<Expr> widget LeadingExpr
            let items = Widgets.getScalarValue widget Items

            if List.isEmpty items then
                invalidArg "items" "DynamicChainExpr needs at least one DynamicMember(...)."

            Expr.DynamicChain(ExprDynamicChainNode(leadingExpr, items, Range.range0)))

[<AutoOpen>]
module DynamicChainBuilders =
    type Ast with

        /// <summary>
        /// Create one <c>?name</c> step of a dynamic lookup chain, for use in <c>DynamicChainExpr</c>.
        /// </summary>
        /// <param name="name">The member looked up.</param>
        static member DynamicMember(name: string) =
            WidgetBuilder<ExprDynamicChainItemNode>(
                DynamicChainItem.WidgetKey,
                DynamicChainItem.MemberExpr.WithValue(Ast.ConstantExpr(Ast.Constant(name)).Compile())
            )

        /// <summary>
        /// Create one <c>?name(arg)</c> step of a dynamic lookup chain, for use in <c>DynamicChainExpr</c>.
        /// An argument that is not already parenthesized or unit is wrapped in parentheses.
        /// </summary>
        /// <param name="name">The member looked up.</param>
        /// <param name="arg">The argument applied to the member.</param>
        static member DynamicMember(name: string, arg: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprDynamicChainItemNode>(
                DynamicChainItem.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| DynamicChainItem.MemberExpr.WithValue(Ast.ConstantExpr(Ast.Constant(name)).Compile())
                       DynamicChainItem.ArgExpr.WithValue(arg.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create one <c>?name(arg)</c> step of a dynamic lookup chain, for use in <c>DynamicChainExpr</c>.
        /// </summary>
        /// <param name="name">The member looked up.</param>
        /// <param name="arg">The argument applied to the member.</param>
        static member DynamicMember(name: string, arg: WidgetBuilder<Constant>) =
            Ast.DynamicMember(name, Ast.ConstantExpr(arg))

        /// <summary>
        /// Create one <c>?name(arg)</c> step of a dynamic lookup chain, for use in <c>DynamicChainExpr</c>.
        /// </summary>
        /// <param name="name">The member looked up.</param>
        /// <param name="arg">The argument applied to the member.</param>
        static member DynamicMember(name: string, arg: string) =
            Ast.DynamicMember(name, Ast.Constant(arg))

        /// <summary>
        /// Create a chain of dynamic lookups (<c>x?a?b(1)</c>), which requires a user-defined <c>?</c> operator.
        /// Use <c>DynamicExpr</c> for a single lookup. A leading expression that is an application
        /// needs parentheses (<c>ParenExpr(...)</c>), otherwise the lookup binds to its last argument.
        /// </summary>
        /// <param name="value">The expression the chain starts from.</param>
        /// <param name="items">The <c>DynamicMember(...)</c> steps, at least one.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("v", DynamicChainExpr("x", [ DynamicMember("a"); DynamicMember("b", Int(1)) ]))
        ///     }
        /// }
        /// </code>
        static member DynamicChainExpr(value: WidgetBuilder<Expr>, items: WidgetBuilder<ExprDynamicChainItemNode> seq) =
            WidgetBuilder<Expr>(
                DynamicChain.WidgetKey,
                AttributesBundle(
                    StackList.one(DynamicChain.Items.WithValue(items |> Seq.map Gen.mkOak |> List.ofSeq)),
                    [| DynamicChain.LeadingExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a chain of dynamic lookups (<c>x?a?b(1)</c>).</summary>
        /// <param name="value">The expression the chain starts from.</param>
        /// <param name="items">The <c>DynamicMember(...)</c> steps, at least one.</param>
        static member DynamicChainExpr
            (value: WidgetBuilder<Constant>, items: WidgetBuilder<ExprDynamicChainItemNode> seq)
            =
            Ast.DynamicChainExpr(Ast.ConstantExpr(value), items)

        /// <summary>Create a chain of dynamic lookups (<c>x?a?b(1)</c>).</summary>
        /// <param name="value">The expression the chain starts from.</param>
        /// <param name="items">The <c>DynamicMember(...)</c> steps, at least one.</param>
        static member DynamicChainExpr(value: string, items: WidgetBuilder<ExprDynamicChainItemNode> seq) =
            Ast.DynamicChainExpr(Ast.Constant(value), items)
