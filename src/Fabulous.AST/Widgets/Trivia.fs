namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module CommentOnSingleLine =
    let Value = Attributes.defineScalar<string> "Comment"

    let WidgetKey =
        Widgets.register "CommentOnSingleLine" (fun widget ->
            let comment = Widgets.getScalarValue widget Value
            TriviaContent.CommentOnSingleLine("// " + comment))

[<AutoOpen>]
module CommentOnSingleLineBuilders =
    type Ast with
        /// <summary>Creates a single line comment.</summary>
        /// <param name="text">The comment text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///         .triviaBefore(SingleLine("This is a comment"))
        /// }
        /// </code>
        static member SingleLine(text: string) =
            WidgetBuilder<TriviaContent>(CommentOnSingleLine.WidgetKey, CommentOnSingleLine.Value.WithValue(text))

module TriviaNode =

    let Content = Attributes.defineWidget "Content"

    let WidgetKey =
        Widgets.register "TriviaNode" (fun widget ->
            let content = Widgets.getNodeFromWidget<TriviaContent> widget Content
            TriviaNode(content, Range.Zero))

[<AutoOpen>]
module TriviaNodeNodeBuilder =
    type Ast with
        /// <summary>Creates a trivia node with the specified content.</summary>
        /// <param name="content">The trivia content.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///         .triviaBefore(TriviaNode(SingleLine("Comment before")))
        /// }
        /// </code>
        static member TriviaNode(content: WidgetBuilder<TriviaContent>) =
            WidgetBuilder<TriviaNode>(TriviaNode.WidgetKey, TriviaNode.Content.WithValue(content.Compile()))

module LineCommentAfterSourceCode =
    let Value = Attributes.defineScalar<string> "Comment"

    let WidgetKey =
        Widgets.register "LineCommentAfterSourceCode" (fun widget ->
            let comment = Widgets.getScalarValue widget Value
            TriviaContent.LineCommentAfterSourceCode("// " + comment))

[<AutoOpen>]
module LineCommentAfterSourceCodeBuilders =
    type Ast with
        /// <summary>Creates a line comment that appears after source code on the same line.</summary>
        /// <param name="text">The comment text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///     |> _.triviaAfter(LineCommentAfterSourceCode("This is a comment"))
        /// }
        /// </code>
        static member LineCommentAfterSourceCode(text: string) =
            WidgetBuilder<TriviaContent>(
                LineCommentAfterSourceCode.WidgetKey,
                LineCommentAfterSourceCode.Value.WithValue(text)
            )

module BlockComment =
    let Comment = Attributes.defineScalar<string> "Comment"
    let NewlineBefore = Attributes.defineScalar<bool> "NewlineBefore"
    let NewlineAfter = Attributes.defineScalar<bool> "NewlineAfter"

    let WidgetKey =
        Widgets.register "BlockComment" (fun widget ->
            let comment = Widgets.getScalarValue widget Comment
            let newlineBefore = Widgets.getScalarValue widget NewlineBefore
            let newlineAfter = Widgets.getScalarValue widget NewlineAfter

            let comment =
                match newlineBefore, newlineAfter with
                | true, true -> "(*\n" + comment + "\n*)"
                | true, false -> "(*\n" + comment + "*)"
                | false, true -> "(*" + comment + "\n*)"
                | false, false -> "(*" + comment + "*)"

            TriviaContent.BlockComment(comment, newlineBefore, newlineAfter))

[<AutoOpen>]
module BlockCommentBuilders =
    type Ast with
        /// <summary>Creates a block comment with optional newlines.</summary>
        /// <param name="comment">The comment text.</param>
        /// <param name="newlineBefore">Whether to add a newline before the comment content.</param>
        /// <param name="newlineAfter">Whether to add a newline after the comment content.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///     |> _.triviaBefore(BlockComment("This is a block comment", true, true))
        /// }
        /// </code>
        static member BlockComment(comment: string, ?newlineBefore: bool, ?newlineAfter: bool) =
            let newlineBefore = defaultArg newlineBefore false
            let newlineAfter = defaultArg newlineAfter false

            WidgetBuilder<TriviaContent>(
                BlockComment.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        BlockComment.Comment.WithValue(comment),
                        BlockComment.NewlineBefore.WithValue(newlineBefore),
                        BlockComment.NewlineAfter.WithValue(newlineAfter)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

module Directive =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Directive" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            TriviaContent.Directive(value))

[<AutoOpen>]
module DirectiveBuilders =
    type Ast with
        /// <summary>Creates a directive trivia.</summary>
        /// <param name="value">The directive text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///     |> _.triviaBefore(Directive("#if DEBUG"))
        /// }
        /// </code>
        static member Directive(value: string) =
            WidgetBuilder<TriviaContent>(Directive.WidgetKey, Directive.Value.WithValue(value))

module Newline =
    let WidgetKey = Widgets.register "Newline" (fun _ -> TriviaContent.Newline)

[<AutoOpen>]
module NewlineBuilders =
    type Ast with
        /// <summary>Creates a newline trivia.</summary>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Value("x", "1")
        ///     |> _.triviaAfter(Newline())
        /// }
        /// </code>
        static member Newline() =
            WidgetBuilder<TriviaContent>(Newline.WidgetKey)

type TriviaNodeModifiers =
    /// <summary>Adds trivia before the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">List of trivia content to add before the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaBefore([ SingleLine("Comment before"); BlockComment("Block comment") ])
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent> seq) =
        let node = Gen.mkOak this

        for content in value do
            let content = Ast.TriviaNode(content) |> Gen.mkOak
            node.AddBefore(content)

        Ast.EscapeHatch(node)

    /// <summary>Adds trivia before the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">List of trivia content to add before the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaBefore([ TriviaNode(SingleLine("Comment before")); TriviaNode(BlockComment("Block comment")) ])
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaNode> seq) =
        let node = Gen.mkOak this

        for content in value do
            let content = content |> Gen.mkOak
            node.AddBefore(content)

        Ast.EscapeHatch(node)

    /// <summary>Adds trivia before the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Trivia content to add before the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaBefore(TriviaNode(SingleLine("Comment before")))
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent>) =
        TriviaNodeModifiers.triviaBefore(this, [ value ])

    /// <summary>Adds trivia before the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Trivia content to add before the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaBefore(SingleLine("Comment before"))
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaNode>) =
        TriviaNodeModifiers.triviaBefore(this, [ value ])

    /// <summary>Adds trivia after the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">List of trivia content to add after the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaAfter([ SingleLine("Comment after"); Newline() ])
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaAfter(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent> seq) =
        let node = Gen.mkOak this

        for content in value do
            let content = Ast.TriviaNode(content) |> Gen.mkOak
            node.AddAfter(content)

        Ast.EscapeHatch(node)

    /// <summary>Adds trivia after the current node.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Trivia content to add after the node.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Value("x", "1")
    ///     |> _.triviaAfter(SingleLine("Comment after"))
    /// }
    /// </code>
    [<Extension>]
    static member inline triviaAfter(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent>) =
        TriviaNodeModifiers.triviaAfter(this, [ value ])
