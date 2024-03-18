namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeTuple =
    let Path = Attributes.defineWidget "Path"

    let Exponent = Attributes.defineWidget "Exponent"

    let WidgetKey =
        Widgets.register "TypeTuple" (fun widget ->
            let path = Widgets.getNodeFromWidget<Type> widget Path
            let exponent = Widgets.getNodeFromWidget<Type> widget Exponent

            Type.Tuple(
                TypeTupleNode(
                    [ Choice1Of2(path)
                      Choice1Of2(
                          Type.LongIdent(
                              IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.forwardSlash) ], Range.Zero)
                          )
                      )
                      Choice1Of2(exponent) ],
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeTupleBuilders =
    type Ast with
        static member Tuple(first: WidgetBuilder<Type>, exponent: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeTuple.WidgetKey,
                AttributesBundle(
                    StackList.empty(),

                    [| TypeTuple.Path.WithValue(first.Compile())
                       TypeTuple.Exponent.WithValue(exponent.Compile()) |],
                    Array.empty
                )
            )

        static member Tuple(first: string, exponent: string) =
            Ast.Tuple(Ast.LongIdent(first), Ast.LongIdent(exponent))

        static member Tuple(first: string, second: string, exponent: WidgetBuilder<Type>) =
            Ast.Tuple(Ast.AppPostfix(Ast.LongIdent first, Ast.LongIdent second), exponent)

        static member Tuple(first: string list, second: string list, exponent: WidgetBuilder<Type>) =
            Ast.Tuple(Ast.AppPostfix(Ast.LongIdent first, Ast.LongIdent second), exponent)
