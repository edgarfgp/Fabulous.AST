namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeAppPostfix =
    let First = Attributes.defineWidget "Type"

    let Last = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeAppPostfix" (fun widget ->
            let first = Widgets.getNodeFromWidget<Type> widget First
            let last = Widgets.getNodeFromWidget<Type> widget Last
            Type.AppPostfix(TypeAppPostFixNode(first, last, Range.Zero)))

[<AutoOpen>]
module TypeAppPostfixBuilders =
    type Ast with
        static member AppPostfix(first: WidgetBuilder<Type>, last: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeAppPostfix.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeAppPostfix.First.WithValue(first.Compile())
                       TypeAppPostfix.Last.WithValue(last.Compile()) |],
                    Array.empty
                )
            )

        static member AppPostfix(first: string, last: WidgetBuilder<Type>) =
            Ast.AppPostfix(Ast.LongIdent first, last)

        static member AppPostfix(first: WidgetBuilder<Type>, last: string) =
            Ast.AppPostfix(first, Ast.LongIdent last)

        static member AppPostfix(first: string, last: string) =
            Ast.AppPostfix(Ast.LongIdent first, Ast.LongIdent last)

        static member OptionPostfix(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("option"))

        static member OptionPostfix(first: string) =
            Ast.AppPostfix(first, Ast.LongIdent("option"))

        static member VOptionPostfix(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("voption"))

        static member VOptionPostfix(first: string) =
            Ast.AppPostfix(first, Ast.LongIdent("voption"))

        static member ListPostfix(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("list"))

        static member ListPostfix(first: string) =
            Ast.AppPostfix(first, Ast.LongIdent("list"))

        static member SeqPostfix(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("seq"))

        static member SeqPostfix(first: string) =
            Ast.AppPostfix(first, Ast.LongIdent("seq"))
