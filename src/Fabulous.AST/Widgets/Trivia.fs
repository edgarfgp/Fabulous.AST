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
        static member Directive(value: string) =
            WidgetBuilder<TriviaContent>(Directive.WidgetKey, Directive.Value.WithValue(value))

module Newline =
    let WidgetKey = Widgets.register "Newline" (fun _ -> TriviaContent.Newline)

[<AutoOpen>]
module NewlineBuilders =
    type Ast with
        static member Newline() =
            WidgetBuilder<TriviaContent>(Newline.WidgetKey)

type TriviaNodeModifiers =
    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent> list) =
        let node = Gen.mkOak this

        for content in value do
            let content = Ast.TriviaNode(content) |> Gen.mkOak
            node.AddBefore(content)

        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline triviaBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent>) =
        TriviaNodeModifiers.triviaBefore(this, [ value ])

    [<Extension>]
    static member inline triviaAfter(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent> list) =
        let node = Gen.mkOak this

        for content in value do
            let content = Ast.TriviaNode(content) |> Gen.mkOak
            node.AddAfter(content)

        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline triviaAfter(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaContent>) =
        TriviaNodeModifiers.triviaAfter(this, [ value ])
