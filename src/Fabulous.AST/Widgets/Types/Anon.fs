namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeAnon =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeAnon" (fun widget ->
            let identifier = Helpers.getScalarValue widget Identifier
            Type.Anon(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeAnonBuilders =
    type Ast with
        static member TypeAnon(identifier: string) =
            WidgetBuilder<Type>(
                TypeAnon.WidgetKey,
                AttributesBundle(StackList.one(TypeAnon.Identifier.WithValue(identifier)), ValueNone, ValueNone)
            )
