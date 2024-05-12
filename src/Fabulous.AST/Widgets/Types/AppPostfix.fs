namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

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

        static member Option(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("option"))

        static member VOption(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("option"))

        static member List(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("list"))

        static member Seq(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("seq"))

        static member Array(first: WidgetBuilder<Type>) =
            Ast.AppPostfix(first, Ast.LongIdent("array"))
