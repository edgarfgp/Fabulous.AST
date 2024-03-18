namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeVar =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeVar" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            Type.Var(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeVarBuilders =
    type Ast with
        static member Var(value: string) =
            WidgetBuilder<Type>(
                TypeVar.WidgetKey,
                AttributesBundle(StackList.one(TypeVar.Identifier.WithValue(value)), Array.empty, Array.empty)
            )
