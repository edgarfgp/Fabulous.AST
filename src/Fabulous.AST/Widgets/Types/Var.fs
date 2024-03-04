namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeVar =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeVar" (fun widget ->
            let identifier = Helpers.getScalarValue widget Identifier
            Type.Var(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeVarBuilders =
    type Ast with
        static member TypeVar(identifier: string) =
            WidgetBuilder<Type>(
                TypeVar.WidgetKey,
                AttributesBundle(StackList.one(TypeVar.Identifier.WithValue(identifier)), ValueNone, ValueNone)
            )