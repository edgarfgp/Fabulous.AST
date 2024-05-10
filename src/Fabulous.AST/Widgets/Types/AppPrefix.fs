namespace Fabulous.AST

open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeAppPrefix =
    let Identifier = Attributes.defineWidget "Identifier"

    let PostIdentifier = Attributes.defineScalar<string list> "PostIdentifier"

    let Arguments = Attributes.defineScalar<Type list> "Arguments"

    let WidgetKey =
        Widgets.register "TypeAppPrefix" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Type> widget Identifier
            let postIdentifier = Widgets.tryGetScalarValue widget PostIdentifier

            let postIdentifier =
                match postIdentifier with
                | ValueSome postIdentifier ->
                    IdentListNode(
                        [ for identifier in postIdentifier do
                              let identifier = PrettyNaming.NormalizeIdentifierBackticks identifier
                              IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ],
                        Range.Zero
                    )
                    |> Some
                | ValueNone -> None

            let arguments = Widgets.getScalarValue widget Arguments

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
        static member AppPrefix(t: WidgetBuilder<Type>, arguments: WidgetBuilder<Type> list) =
            let arguments = arguments |> List.map Gen.mkOak

            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeAppPrefix.Arguments.WithValue(arguments)),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: string, arguments: WidgetBuilder<Type> list) =
            Ast.AppPrefix(Ast.LongIdent t, arguments)

        static member AppPrefix(t: string, arguments: string list) =
            let arguments = arguments |> List.map Ast.LongIdent
            Ast.AppPrefix(Ast.LongIdent t, arguments)

        static member AppPrefix(t: WidgetBuilder<Type>, arguments: string list) =
            let arguments = arguments |> List.map Ast.LongIdent
            Ast.AppPrefix(t, arguments)

        static member AppPrefix(t: string, arguments: string) =
            Ast.AppPrefix(Ast.LongIdent t, [ Ast.LongIdent arguments ])

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string list, argument: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeAppPrefix.Arguments.WithValue([ Gen.mkOak argument ]),
                        TypeAppPrefix.PostIdentifier.WithValue(postIdentifier)
                    ),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: string, postIdentifier: string list, argument: WidgetBuilder<Type>) =
            Ast.AppPrefix(Ast.LongIdent t, postIdentifier, argument)

        static member AppPrefix(t: string, postIdentifier: string list, arguments: string) =
            Ast.AppPrefix(Ast.LongIdent t, postIdentifier, Ast.LongIdent arguments)
