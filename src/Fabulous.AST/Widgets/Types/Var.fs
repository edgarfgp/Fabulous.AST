namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax

module TypeVar =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeVar" (fun widget ->
            let identifier =
                Widgets.getScalarValue widget Identifier
                |> PrettyNaming.NormalizeIdentifierBackticks

            Type.Var(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeVarBuilders =
    type Ast with
        static member Var(value: string) =
            WidgetBuilder<Type>(
                TypeVar.WidgetKey,
                AttributesBundle(StackList.one(TypeVar.Identifier.WithValue(value)), Array.empty, Array.empty)
            )
