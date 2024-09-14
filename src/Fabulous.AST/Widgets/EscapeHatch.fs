namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module EscapeHatch =
    let Node = Attributes.defineScalar<obj> "Node"

    let WidgetKey =
        Widgets.register "EscapeHatch" (fun widget ->
            let node = Widgets.getScalarValue widget Node
            node)

[<AutoOpen>]
module EscapeHatchBuilders =
    type Ast with

        /// <summary>
        /// Creates a widget that holds a reference to a SyntaxTree node so it can be composed in the widget tree.
        /// </summary>
        /// <param name="node">The node to hold a reference to</param>
        /// <code language="fsharp">
        /// EscapeHatch(BindingNode(...))
        /// </code>
        static member inline EscapeHatch(node: 'T) =
            WidgetBuilder<'T>(EscapeHatch.WidgetKey, EscapeHatch.Node.WithValue(node))

type EscapeHatchModifiers =
    [<Extension>]
    static member inline lineCommentAfter(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(CommentOnSingleLine("// " + value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline lineCommentAfter(this: WidgetBuilder<#NodeBase>, value: string list) =
        let node = Gen.mkOak this
        let values = [ for v in value -> ("// " + v) ] |> List.toArray |> String.concat "\n"
        node.AddAfter(TriviaNode(CommentOnSingleLine(values), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline lineCommentAfterSource(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(LineCommentAfterSourceCode("// " + value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline blockCommentAfter(this: WidgetBuilder<#NodeBase>, value: string, newlineBefore: bool, newlineAfter: bool) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(BlockComment(value, newlineBefore, newlineAfter), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline newlineAfter(this: WidgetBuilder<#NodeBase>) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(Newline, Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline directiveAfter(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(Directive(value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline cursorAfter(this: WidgetBuilder<#NodeBase>) =
        let node = Gen.mkOak this
        node.AddAfter(TriviaNode(Cursor, Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline lineCommentBefore(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(CommentOnSingleLine("// " + value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline lineCommentBefore(this: WidgetBuilder<#NodeBase>, value: string list) =
        let node = Gen.mkOak this
        let values = [ for v in value -> ("// " + v) ] |> List.toArray |> String.concat "\n"
        node.AddBefore(TriviaNode(CommentOnSingleLine(values), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline lineCommentBeforeSource(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(LineCommentAfterSourceCode("// " + value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline blockCommentBefore(this: WidgetBuilder<#NodeBase>, value: string, newlineBefore: bool, newlineAfter: bool) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(BlockComment(value, newlineBefore, newlineAfter), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline newlineBefore(this: WidgetBuilder<#NodeBase>) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(Newline, Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline directiveBefore(this: WidgetBuilder<#NodeBase>, value: string) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(Directive(value), Range.Zero))
        Ast.EscapeHatch(node)

    [<Extension>]
    static member inline cursorBefore(this: WidgetBuilder<#NodeBase>) =
        let node = Gen.mkOak this
        node.AddBefore(TriviaNode(Cursor, Range.Zero))
        Ast.EscapeHatch(node)
