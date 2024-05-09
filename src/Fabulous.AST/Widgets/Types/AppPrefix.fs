namespace Fabulous.AST

open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeAppPrefix =
    let Identifier = Attributes.defineWidget "Identifier"

    let PostIdentifier = Attributes.defineScalar<string list> "PostIdentifier"

    let Arguments = Attributes.defineWidget "Arguments"

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

            let arguments = Widgets.getNodeFromWidget<Type> widget Arguments

            Type.AppPrefix(
                TypeAppPrefixNode(
                    identifier,
                    postIdentifier,
                    SingleTextNode.lessThan,
                    [ arguments ],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeAppPrefixBuilders =
    type Ast with
        static member AppPrefix(t: WidgetBuilder<Type>, arguments: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile())
                       TypeAppPrefix.Arguments.WithValue(arguments.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: string, arguments: string) =
            Ast.AppPrefix(Ast.LongIdent t, Ast.LongIdent arguments)

        static member AppPrefix(t: WidgetBuilder<Type>, postIdentifier: string list, arguments: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeAppPrefix.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeAppPrefix.PostIdentifier.WithValue(postIdentifier)),
                    [| TypeAppPrefix.Identifier.WithValue(t.Compile())
                       TypeAppPrefix.Arguments.WithValue(arguments.Compile()) |],
                    Array.empty
                )
            )

        static member AppPrefix(t: string, postIdentifier: string list, arguments: string) =
            Ast.AppPrefix(Ast.LongIdent t, postIdentifier, Ast.LongIdent arguments)
