namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module RecordField =
    let RecordExpr = Attributes.defineWidget "RecordExpr"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "RecordField" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget RecordExpr

            let name =
                Widgets.getScalarValue widget Name
                |> PrettyNaming.NormalizeIdentifierBackticks
                |> SingleTextNode.Create

            RecordFieldNode(
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.range0),
                SingleTextNode.equals,
                expr,
                Range.range0
            ))

[<AutoOpen>]
module RecordFieldBuilders =
    type Ast with

        static member RecordFieldExpr(name: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<RecordFieldNode>(
                RecordField.WidgetKey,
                AttributesBundle(
                    StackList.one(RecordField.Name.WithValue(name)),
                    [| RecordField.RecordExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member RecordFieldExpr(name: string, expr: WidgetBuilder<Constant>) =
            Ast.RecordFieldExpr(name, Ast.ConstantExpr(expr))

        static member RecordFieldExpr(name: string, expr: string) =
            Ast.RecordFieldExpr(name, Ast.Constant(expr))

module RecordSpread =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "RecordSpread" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            ExprSpreadNode(SingleTextNode.Create("..."), expr, Range.range0))

[<AutoOpen>]
module RecordSpreadBuilders =
    type Ast with

        /// <summary>
        /// Create a spread (<c>...source</c>) that copies the fields of an existing value into a record
        /// or anonymous record expression. Yield it in the body of <c>RecordExpr()</c>,
        /// <c>AnonRecordExpr()</c> or <c>AnonStructRecordExpr()</c>. Record spreads need F# 11 or later.
        /// </summary>
        /// <param name="value">The value whose fields are copied.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("p2", RecordExpr() {
        ///             SpreadExpr("p1")
        ///             RecordFieldExpr("X", Int(1))
        ///         })
        ///     }
        /// }
        /// </code>
        static member SpreadExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprSpreadNode>(RecordSpread.WidgetKey, RecordSpread.Value.WithValue(value.Compile()))

        /// <summary>Create a spread (<c>...source</c>) of the named value. Record spreads need F# 11 or later.</summary>
        /// <param name="value">The value whose fields are copied.</param>
        static member SpreadExpr(value: WidgetBuilder<Constant>) = Ast.SpreadExpr(Ast.ConstantExpr(value))

        /// <summary>Create a spread (<c>...source</c>) of the named value. Record spreads need F# 11 or later.</summary>
        /// <param name="value">The value whose fields are copied.</param>
        static member SpreadExpr(value: string) = Ast.SpreadExpr(Ast.Constant(value))
