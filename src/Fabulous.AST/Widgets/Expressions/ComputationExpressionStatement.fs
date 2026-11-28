namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ComputationExpressionStatement =
    let Binding = Attributes.defineWidget "Value"
    let InKeyword = Attributes.defineScalar<bool> "InKeyword"
    let ExprValue = Attributes.defineWidget "OtherExpr"
    let PatternValue = Attributes.defineWidget "Pattern"

    // Fantomas 8 models every binding inside a computation expression (`let`, `use`, `let!`,
    // `use!`, `and!`) as a plain BindingNode whose leading keyword says which one it is.
    let private withInKeyword (inKeyword: SingleTextNode option) (b: BindingNode) =
        BindingNode(
            b.XmlDoc,
            b.Attributes,
            b.LeadingKeyword,
            b.IsMutable,
            b.Inline,
            b.Accessibility,
            b.FunctionName,
            b.GenericTypeParameters,
            b.Parameters,
            b.ReturnType,
            b.Equals,
            b.Expr,
            inKeyword,
            b.Range
        )

    let private inKeyword(widget: Widget) =
        match Widgets.tryGetScalarValue widget InKeyword with
        | ValueSome true -> Some SingleTextNode.``in``
        | _ -> None

    let private bangBinding (keyword: SingleTextNode) (widget: Widget) =
        let pat = Widgets.getNodeFromWidget<Pattern> widget PatternValue
        let expr = Widgets.getNodeFromWidget<Expr> widget ExprValue

        BindingNode(
            None,
            None,
            MultipleTextsNode([ keyword ], Range.range0),
            false,
            None,
            None,
            Choice2Of2 pat,
            None,
            [],
            None,
            SingleTextNode.equals,
            expr,
            inKeyword widget,
            Range.range0
        )

    let WidgetLetOrUseStatementKey =
        Widgets.register "LetOrUseStatement" (fun widget ->
            let binding = Widgets.getNodeFromWidget<BindingNode> widget Binding

            let binding =
                match inKeyword widget with
                | Some _ as node -> withInKeyword node binding
                | None -> binding

            ComputationExpressionStatement.BindingStatement(binding))

    let WidgetOtherStatementKey =
        Widgets.register "OtherStatement" (fun widget ->
            let otherExpr = Widgets.getNodeFromWidget<Expr> widget ExprValue
            ComputationExpressionStatement.OtherStatement(otherExpr))

    let WidgetLetOrUseBangStatementKey =
        Widgets.register "LetOrUseBangStatement" (fun widget ->
            ComputationExpressionStatement.BindingStatement(bangBinding SingleTextNode.letBang widget))

    let WidgetUseBangStatementKey =
        Widgets.register "UseBangStatement" (fun widget ->
            ComputationExpressionStatement.BindingStatement(bangBinding SingleTextNode.useBang widget))

    let WidgetAndBangStatementKey =
        Widgets.register "AndBangStatement" (fun widget ->
            ComputationExpressionStatement.BindingStatement(bangBinding SingleTextNode.andBang widget))

[<AutoOpen>]
module ComputationExpressionStatementBuilders =
    type Ast with

        static member LetOrUseExpr(value: WidgetBuilder<BindingNode>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetLetOrUseStatementKey,
                ComputationExpressionStatement.Binding.WithValue(value.Compile())
            )

        static member OtherExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetOtherStatementKey,
                ComputationExpressionStatement.ExprValue.WithValue(value.Compile())
            )

        static member OtherExpr(value: WidgetBuilder<Constant>) = Ast.OtherExpr(Ast.ConstantExpr(value))

        static member OtherExpr(value: string) = Ast.OtherExpr(Ast.ConstantExpr(value))

        static member LetOrUseBangExpr(pat: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetLetOrUseBangStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.PatternValue.WithValue(pat.Compile())
                       ComputationExpressionStatement.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member LetOrUseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.LetOrUseBangExpr(Ast.ConstantPat(pat), expr)

        static member LetOrUseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.LetOrUseBangExpr(Ast.ConstantPat(pat), Ast.ConstantExpr(expr))

        static member LetOrUseBangExpr(pat: string, expr: string) =
            Ast.LetOrUseBangExpr(Ast.Constant(pat), Ast.Constant(expr))

        /// <summary>Creates a <c>use!</c> binding inside a computation expression; the bound value is
        /// disposed when it goes out of scope.</summary>
        /// <param name="pat">The pattern to bind.</param>
        /// <param name="expr">The computation whose result is bound.</param>
        /// <code language="fsharp">
        /// NamedComputationExpr("task", CompExprBodyExpr([ UseBangExpr("conn", "openAsync ()"); OtherExpr("conn.Query()") ]))
        /// </code>
        static member UseBangExpr(pat: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetUseBangStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.PatternValue.WithValue(pat.Compile())
                       ComputationExpressionStatement.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member UseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.UseBangExpr(Ast.ConstantPat(pat), expr)

        static member UseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.UseBangExpr(Ast.ConstantPat(pat), Ast.ConstantExpr(expr))

        static member UseBangExpr(pat: string, expr: string) =
            Ast.UseBangExpr(Ast.Constant(pat), Ast.Constant(expr))

        static member AndBangExpr(pat: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetAndBangStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.PatternValue.WithValue(pat.Compile())
                       ComputationExpressionStatement.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member AndBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.AndBangExpr(Ast.ConstantPat(pat), expr)

        static member AndBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.AndBangExpr(pat, Ast.ConstantExpr(expr))

        static member AndBangExpr(pat: string, expr: string) =
            Ast.AndBangExpr(Ast.Constant(pat), Ast.Constant(expr))

type ComputationExpressionStatementModifiers =
    /// <summary>
    /// Ends a <c>let</c>, <c>use</c>, <c>let!</c>, <c>use!</c> or <c>and!</c> binding with <c>in</c>
    /// (<c>let! x = fetch () in</c>). Other statements ignore it.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// NamedComputationExpr("async", CompExprBodyExpr([ LetOrUseBangExpr("x", "fetch ()").withIn(); OtherExpr("return x") ]))
    /// </code>
    [<Extension>]
    static member inline withIn(this: WidgetBuilder<ComputationExpressionStatement>) =
        this.AddScalar(ComputationExpressionStatement.InKeyword.WithValue(true))
