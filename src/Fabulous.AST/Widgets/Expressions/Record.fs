namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RecordExpr =
    let Fields = Attributes.defineScalar<RecordFieldNode seq> "Fields"

    /// The fields and spreads of a record expression built with the computation expression form, in source order.
    let Items = Attributes.defineWidgetCollection "Items"
    let CopyInfo = Attributes.defineWidget "CopyInfo"

    let OpenBrace = Attributes.defineScalar<SingleTextNode> "OpenBrace"

    let CloseBrace = Attributes.defineScalar<SingleTextNode> "CloseBrace"

    /// The fields given as a list, followed by the fields and spreads yielded in the body.
    let items(widget: Widget) =
        let fields =
            Widgets.tryGetScalarValue widget Fields
            |> ValueOption.map(Seq.map ExprRecordFieldOrSpread.Field >> List.ofSeq)
            |> ValueOption.defaultValue []

        let yielded =
            Widgets.tryGetNodesFromWidgetCollection<obj> widget Items
            |> ValueOption.defaultValue []
            |> List.map(fun item ->
                match item with
                | :? RecordFieldNode as node -> ExprRecordFieldOrSpread.Field node
                | :? ExprSpreadNode as node -> ExprRecordFieldOrSpread.Spread node
                | other ->
                    invalidArg "item" $"Record expressions accept fields and spreads. Got: %s{other.GetType().Name}")

        fields @ yielded

    let WidgetKey =
        Widgets.register "RecordExpr" (fun widget ->
            let openBrace = Widgets.getScalarValue widget OpenBrace
            let closeBrace = Widgets.getScalarValue widget CloseBrace
            let copyInfo = Widgets.tryGetNodeFromWidget<Expr> widget CopyInfo

            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> Some copyInfo
                | ValueNone -> None

            let fields = items widget

            Expr.Record(ExprRecordNode(openBrace, copyInfo, fields, closeBrace, Range.range0)))

[<AutoOpen>]
module RecordExprBuilders =
    type Ast with

        static member private BaseRecordExpr
            (
                copyInfo: WidgetBuilder<Expr> voption,
                leftSingleNode: SingleTextNode,
                rightSingleNode: SingleTextNode,
                fields: WidgetBuilder<RecordFieldNode> seq
            ) =
            let copyInfo =
                match copyInfo with
                | ValueSome copyInfo -> [| RecordExpr.CopyInfo.WithValue(copyInfo.Compile()) |]
                | ValueNone -> Array.empty

            WidgetBuilder<Expr>(
                RecordExpr.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        RecordExpr.OpenBrace.WithValue(leftSingleNode),
                        RecordExpr.CloseBrace.WithValue(rightSingleNode),
                        RecordExpr.Fields.WithValue(fields |> Seq.map Gen.mkOak |> Seq.toArray)
                    ),
                    copyInfo,
                    Array.empty
                )
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBrace,
                SingleTextNode.rightCurlyBrace,
                fields
            )

        static member RecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.RecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member RecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.RecordExpr(Ast.Constant(copyInfo), fields)

        static member RecordExpr(fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(ValueNone, SingleTextNode.leftCurlyBrace, SingleTextNode.rightCurlyBrace, fields)

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Expr>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueSome copyInfo,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )

        static member AnonRecordExpr(copyInfo: WidgetBuilder<Constant>, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.AnonRecordExpr(Ast.ConstantExpr(copyInfo), fields)

        static member AnonRecordExpr(copyInfo: string, fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.AnonRecordExpr(Ast.Constant(copyInfo), fields)

        static member AnonRecordExpr(fields: WidgetBuilder<RecordFieldNode> seq) =
            Ast.BaseRecordExpr(
                ValueNone,
                SingleTextNode.leftCurlyBraceWithBar,
                SingleTextNode.rightCurlyBraceWithBar,
                fields
            )

        /// <summary>
        /// Create a record expression whose fields and spreads are given in the body, in order.
        /// A spread (<c>SpreadExpr("r")</c>) copies the fields of an existing value; later items override earlier ones.
        /// Record spreads need F# 11 or later.
        /// </summary>
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
        static member RecordExpr() =
            CollectionBuilder<Expr, RecordFieldNode>(
                RecordExpr.WidgetKey,
                RecordExpr.Items,
                AttributesBundle(
                    StackList.two(
                        RecordExpr.OpenBrace.WithValue(SingleTextNode.leftCurlyBrace),
                        RecordExpr.CloseBrace.WithValue(SingleTextNode.rightCurlyBrace)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>
        /// Create an anonymous record expression whose fields and spreads are given in the body, in order.
        /// Record spreads need F# 11 or later.
        /// </summary>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("p2", AnonRecordExpr() {
        ///             SpreadExpr("p1")
        ///             RecordFieldExpr("Z", Int(3))
        ///         })
        ///     }
        /// }
        /// </code>
        static member AnonRecordExpr() =
            CollectionBuilder<Expr, RecordFieldNode>(
                RecordExpr.WidgetKey,
                RecordExpr.Items,
                AttributesBundle(
                    StackList.two(
                        RecordExpr.OpenBrace.WithValue(SingleTextNode.leftCurlyBraceWithBar),
                        RecordExpr.CloseBrace.WithValue(SingleTextNode.rightCurlyBraceWithBar)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type RecordExprYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<Expr, RecordFieldNode>, x: WidgetBuilder<ExprSpreadNode>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
