namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module LongIdentApp =
    let AppNode = Attributes.defineScalar<struct (Type * string)> "Identifier"

    let WidgetKey =
        Widgets.register "LongIdentApp" (fun widget ->
            let struct (appType, ident) = Widgets.getScalarValue widget AppNode

            Type.LongIdentApp(
                TypeLongIdentAppNode(
                    appType,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ], Range.Zero),
                    Range.Zero
                )
            ))

[<AutoOpen>]
module LongIdentAppBuilders =
    type Ast with
        static member LongIdentApp(appType: WidgetBuilder<Type>, longIdent: string) =
            WidgetBuilder<Type>(LongIdentApp.WidgetKey, LongIdentApp.AppNode.WithValue(Gen.mkOak appType, longIdent))

        static member LongIdentApp(appType: string, longIdent: string) =
            Ast.LongIdentApp(Ast.LongIdent(appType), longIdent)
