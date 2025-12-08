namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TryWith =
    let Value = Attributes.defineWidget "Value"

    let Clauses = Attributes.defineScalar<MatchClauseNode seq> "Clauses"

    let WidgetKey =
        Widgets.register "TryWith" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let clauses = Widgets.getScalarValue widget Clauses |> List.ofSeq
            Expr.TryWith(ExprTryWithNode(SingleTextNode.``try``, expr, SingleTextNode.``with``, clauses, Range.Zero)))

[<AutoOpen>]
module TryWithBuilders =
    type Ast with
        /// <summary>
        /// Create a try-with expression.
        /// </summary>
        /// <param name="value">The expression to try.</param>
        /// <param name="clauses">The match clauses for exception handling.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             ConstantExpr(Int(1)),
        ///             [
        ///                 MatchClause(
        ///                     PatternExpr(PatternVar("e")),
        ///                     ConstantExpr(Int(0))
        ///                 )
        ///             ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            let clauses = clauses |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(
                TryWith.WidgetKey,
                AttributesBundle(
                    StackList.one(TryWith.Clauses.WithValue(clauses)),
                    [| TryWith.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a try-with expression with a constant value.
        /// </summary>
        /// <param name="value">The constant value to try.</param>
        /// <param name="clauses">The match clauses for exception handling.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             Int(1),
        ///             [
        ///                 MatchClause(
        ///                     PatternExpr(PatternVar("e")),
        ///                     ConstantExpr(Int(0))
        ///                 )
        ///             ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: WidgetBuilder<Constant>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), clauses)

        /// <summary>
        /// Create a try-with expression with a string literal value.
        /// </summary>
        /// <param name="value">The string literal to try.</param>
        /// <param name="clauses">The match clauses for exception handling.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             "someFunction()",
        ///             [
        ///                 MatchClause(
        ///                     PatternExpr(PatternVar("e")),
        ///                     ConstantExpr(Int(0))
        ///                 )
        ///             ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: string, clauses: WidgetBuilder<MatchClauseNode> seq) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), clauses)

        /// <summary>
        /// Create a try-with expression with a single match clause.
        /// </summary>
        /// <param name="value">The expression to try.</param>
        /// <param name="clause">The match clause for exception handling.</param>
        static member TryWithExpr(value: WidgetBuilder<Expr>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.TryWithExpr(value, [ clause ])

        /// <summary>
        /// Create a try-with expression with a constant value and a single match clause.
        /// </summary>
        /// <param name="value">The constant value to try.</param>
        /// <param name="clause">The match clause for exception handling.</param>
        static member TryWithExpr(value: WidgetBuilder<Constant>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), clause)

        /// <summary>
        /// Create a try-with expression with a string literal value and a single match clause.
        /// </summary>
        /// <param name="value">The string literal to try.</param>
        /// <param name="clause">The match clause for exception handling.</param>
        static member TryWithExpr(value: string, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.TryWithExpr(Ast.Constant(value), clause)

        /// <summary>
        /// Create a try-with expression with a single match clause for catching all exceptions.
        /// </summary>
        /// <param name="value">The expression to try.</param>
        /// <param name="exceptionName">The name to bind the exception to.</param>
        /// <param name="handler">The expression to handle the exception.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             ConstantExpr(Int(1)),
        ///             "ex",
        ///             ConstantExpr(Int(0))
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: WidgetBuilder<Expr>, exceptionName: string, handler: WidgetBuilder<Expr>) =
            let clause = Ast.MatchClauseExpr(Ast.NamedPat(exceptionName), handler)

            Ast.TryWithExpr(value, [ clause ])

        /// <summary>
        /// Create a try-with expression with a constant value and a single match clause for catching all exceptions.
        /// </summary>
        /// <param name="value">The constant value to try.</param>
        /// <param name="exceptionName">The name to bind the exception to.</param>
        /// <param name="handler">The expression to handle the exception.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             Int(1),
        ///             "ex",
        ///             ConstantExpr(Int(0))
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: WidgetBuilder<Constant>, exceptionName: string, handler: WidgetBuilder<Expr>) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), exceptionName, handler)

        /// <summary>
        /// Create a try-with expression with a string literal value and a single match clause for catching all exceptions.
        /// </summary>
        /// <param name="value">The string literal to try.</param>
        /// <param name="exceptionName">The name to bind the exception to.</param>
        /// <param name="handler">The expression to handle the exception.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TryWithExpr(
        ///             "someFunction()",
        ///             "ex",
        ///             ConstantExpr(Int(0))
        ///         )
        ///     }
        /// }
        /// </code>
        static member TryWithExpr(value: string, exceptionName: string, handler: WidgetBuilder<Expr>) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), exceptionName, handler)
