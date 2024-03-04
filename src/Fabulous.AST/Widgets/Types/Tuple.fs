namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeTuple =
    let Path = Attributes.defineWidget "Path"

    let Exponent = Attributes.defineWidget "Exponent"

    let WidgetKey =
        Widgets.register "TypeTuple" (fun widget ->
            let path = Helpers.getNodeFromWidget<Type> widget Path
            let exponent = Helpers.getNodeFromWidget<Type> widget Exponent

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
        static member TypeTuple(first: WidgetBuilder<Type>, exponent: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeTuple.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeTuple.Path.WithValue(first.Compile())
                           TypeTuple.Exponent.WithValue(exponent.Compile()) |],
                    ValueNone
                )
            )

        static member TypeTuple(first: string, exponent: string) =
            Ast.TypeTuple(Ast.TypeLongIdent(first), Ast.TypeLongIdent(exponent))

        static member TypeTuple(first: string, second: string, exponent: WidgetBuilder<Type>) =
            Ast.TypeTuple(Ast.TypeAppPostfix(Ast.TypeLongIdent first, Ast.TypeLongIdent second), exponent)

        static member TypeTuple(first: string list, second: string list, exponent: WidgetBuilder<Type>) =
            Ast.TypeTuple(Ast.TypeAppPostfix(Ast.TypeLongIdent first, Ast.TypeLongIdent second), exponent)
