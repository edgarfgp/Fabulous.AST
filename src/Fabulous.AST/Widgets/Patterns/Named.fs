namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Named =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Named" (fun widget ->
            let value = Helpers.getScalarValue widget Value
            Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero)))

[<AutoOpen>]
module NamedBuilders =
    type Ast with

        static member NamedPat(value: string) =
            WidgetBuilder<Pattern>(
                Named.WidgetKey,
                AttributesBundle(StackList.one(Named.Value.WithValue(value)), ValueNone, ValueNone)
            )
