namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeAppPrefix =
    let Identifier = Attributes.defineWidget "Identifier"

    let PostIdentifier = Attributes.defineScalar<string seq> "PostIdentifier"

    let Arguments = Attributes.defineScalar<Type seq> "Arguments"

    let WidgetKey =
        Widgets.register "TypeAppPrefix" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Type> widget Identifier
            let postIdentifier = Widgets.tryGetScalarValue widget PostIdentifier

            let postIdentifier =
                match postIdentifier with
                | ValueSome postIdentifier ->
                    IdentListNode(
                        [ for identifier in postIdentifier do
                              IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ],
                        Range.Zero
                    )
                    |> Some
                | ValueNone -> None

            let arguments = Widgets.getScalarValue widget Arguments |> List.ofSeq

            Type.AppPrefix(
                TypeAppPrefixNode(
                    identifier,
                    postIdentifier,
                    SingleTextNode.lessThan,
                    arguments,
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeAppPrefixBuilders =
    type Ast with
        static member AppPrefix(t: WidgetBuilder<Type>, arguments: WidgetBuilder<Type> seq) =
            let arguments = arguments |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeAppPrefix.Arguments.WithValue(arguments)),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: WidgetBuilder<Type>, arguments: string seq) =
            let arguments = arguments |> Seq.map Ast.LongIdent
            Ast.AppPrefix(t, arguments)

        static member AppPrefix(t: WidgetBuilder<Type>, argument: WidgetBuilder<Type>) = Ast.AppPrefix(t, [ argument ])

        static member AppPrefix(t: string, arguments: WidgetBuilder<Type> seq) =
            Ast.AppPrefix(Ast.LongIdent t, arguments)

        static member AppPrefix(t: string, argument: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent t, [ argument ])

        static member AppPrefix(t: string, arguments: string seq) =
            let arguments = arguments |> Seq.map Ast.LongIdent
            Ast.AppPrefix(Ast.LongIdent t, arguments)

        static member AppPrefix(t: string, argument: string) =
            Ast.AppPrefix(Ast.LongIdent t, [ Ast.LongIdent argument ])

        static member AppPrefix
            (t: WidgetBuilder<Type>, postIdentifier: string seq, arguments: WidgetBuilder<Type> seq)
            =
            let arguments = arguments |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeAppPrefix.Arguments.WithValue(arguments),
                        TypeAppPrefix.PostIdentifier.WithValue(postIdentifier)
                    ),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string, argument: WidgetBuilder<Type>) =
            Ast.AppPrefix(t, [ postIdentifier ], [ argument ])

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string, argument: WidgetBuilder<Type> seq) =
            Ast.AppPrefix(t, [ postIdentifier ], argument)

        static member AppPrefix(t: string, postIdentifier: string, argument: WidgetBuilder<Type> seq) =
            Ast.AppPrefix(Ast.LongIdent t, [ postIdentifier ], argument)

        static member AppPrefix(t: string, postIdentifier: string seq, argument: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent t, postIdentifier, [ argument ])

        static member AppPrefix(t: string, postIdentifier: string, argument: WidgetBuilder<Type>) =
            Ast.AppPrefix(t, [ postIdentifier ], argument)

        static member AppPrefix(t: string, postIdentifier: string seq, arguments: string) =
            Ast.AppPrefix(t, postIdentifier, Ast.LongIdent arguments)

        static member AppPrefix(t: string, postIdentifier: string, arguments: string) =
            Ast.AppPrefix(t, [ postIdentifier ], arguments)

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string, arguments: string) =
            Ast.AppPrefix(t, [ postIdentifier ], [ Ast.LongIdent arguments ])

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string seq, arguments: string) =
            Ast.AppPrefix(t, postIdentifier, [ Ast.LongIdent arguments ])

        static member AppPrefix(t: string, postIdentifier: string, arguments: string seq) =
            Ast.AppPrefix(Ast.LongIdent t, [ postIdentifier ], arguments |> Seq.map Ast.LongIdent)

        static member AppPrefix(t: string, postIdentifier: string seq, arguments: string seq) =
            Ast.AppPrefix(Ast.LongIdent t, postIdentifier, arguments |> Seq.map Ast.LongIdent)

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string, arguments: string seq) =
            Ast.AppPrefix(t, [ postIdentifier ], arguments |> Seq.map Ast.LongIdent)

        static member AppPrefix(t: WidgetBuilder<Type>, argument: string) = Ast.AppPrefix(t, [], [ argument ])

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string seq, arguments: string seq) =
            Ast.AppPrefix(t, postIdentifier, arguments |> Seq.map Ast.LongIdent)

        static member ListPrefix(first: WidgetBuilder<Type> seq) =
            Ast.AppPrefix(Ast.LongIdent("seq"), first)

        static member ListPrefix(first: string seq) = Ast.AppPrefix("seq", first)

        static member SeqPrefix(first: WidgetBuilder<Type> seq) =
            Ast.AppPrefix(Ast.LongIdent("seq"), first)

        static member SeqPrefix(first: string seq) = Ast.AppPrefix("seq", first)

        static member OptionPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("option"), [ first ])

        static member OptionPrefix(first: string) = Ast.AppPrefix("option", [ first ])

        static member VOptionPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("voption"), [ first ])

        static member VOptionPrefix(first: string) = Ast.AppPrefix("voption", [ first ])

        static member ArrayPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("array"), [ first ])

        static member ArrayPrefix(first: string) = Ast.AppPrefix("array", [ first ])

        static member ListPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("list"), [ first ])

        static member ListPrefix(first: string) = Ast.AppPrefix("list", [ first ])

        static member SeqPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("seq"), [ first ])

        static member SeqPrefix(first: string) = Ast.AppPrefix("seq", [ first ])

        static member ResultPrefix(first: WidgetBuilder<Type>, last: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("Result"), [ first; last ])

        static member ResultPrefix(first: string, last: WidgetBuilder<Type>) =
            Ast.AppPrefix("Result", [ Ast.LongIdent(first); last ])

        static member ResultPrefix(first: WidgetBuilder<Type>, last: string) =
            Ast.AppPrefix(Ast.LongIdent("Result"), [ first; Ast.LongIdent(last) ])

        static member ResultPrefix(first: string, last: string) =
            Ast.AppPrefix("Result", [ Ast.LongIdent(first); Ast.LongIdent(last) ])

        static member AsyncPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("Async"), [ first ])

        static member AsyncPrefix(first: string) = Ast.AsyncPrefix(Ast.LongIdent(first))

        static member TaskPrefix(first: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent("Task"), [ first ])

        static member TaskPrefix(first: string) = Ast.TaskPrefix(Ast.LongIdent(first))
