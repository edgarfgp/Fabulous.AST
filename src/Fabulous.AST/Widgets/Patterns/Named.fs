namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Named =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Named" (fun widget ->
            let value = Helpers.getScalarValue widget Value

            let name =
                match value with
                | SingleTextNode.WrapWithBackTicks name -> name
                | _ -> SingleTextNode.Create(value)

            Pattern.Named(PatNamedNode(None, name, Range.Zero)))

[<AutoOpen>]
module NamedBuilders =
    type Ast with

        static member NamedPat(value: string) =
            WidgetBuilder<Pattern>(
                Named.WidgetKey,
                AttributesBundle(StackList.one(Named.Value.WithValue(value)), ValueNone, ValueNone)
            )
