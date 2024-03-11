namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Named =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Named" (fun widget ->
            let name =
                Helpers.getScalarValue widget Value
                |> Helpers.normalizeIdentifierBackticks
                |> SingleTextNode.Create

            Pattern.Named(PatNamedNode(None, name, Range.Zero)))

[<AutoOpen>]
module NamedBuilders =
    type Ast with

        static member NamedPat(value: string) =
            WidgetBuilder<Pattern>(
                Named.WidgetKey,
                AttributesBundle(StackList.one(Named.Value.WithValue(value)), ValueNone, ValueNone)
            )
