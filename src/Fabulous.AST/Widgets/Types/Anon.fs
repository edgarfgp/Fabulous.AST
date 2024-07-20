namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax

module TypeAnon =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeAnon" (fun widget ->
            let identifier =
                Widgets.getScalarValue widget Identifier
                |> PrettyNaming.NormalizeIdentifierBackticks

            Type.Anon(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeAnonBuilders =
    type Ast with
        static member Anon(value: string) =
            WidgetBuilder<Type>(
                TypeAnon.WidgetKey,
                AttributesBundle(StackList.one(TypeAnon.Identifier.WithValue(value)), Array.empty, Array.empty)
            )
