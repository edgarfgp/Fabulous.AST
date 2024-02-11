namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module SimplePat =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let IsOptional = Attributes.defineScalar<bool> "Parameters"

    let Typed = Attributes.defineScalar<Type> "Typed"

    let WidgetKey =
        Widgets.register "SimplePatNode" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let isOptional = Helpers.getScalarValue widget IsOptional
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let typed = Helpers.tryGetScalarValue widget Typed

            let typed =
                match typed with
                | ValueSome t -> Some t
                | ValueNone -> None

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            SimplePatNode(multipleAttributes, isOptional, SingleTextNode.Create(name), typed, Range.Zero))

[<AutoOpen>]
module SimplePatNodeBuilders =
    type Ast with

        static member SimplePat(name: string, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.two(SimplePat.Name.WithValue(name), SimplePat.IsOptional.WithValue(isOptional)),
                    ValueNone,
                    ValueNone
                )
            )

        static member SimplePat(name: string, tp: Type, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        SimplePat.Name.WithValue(name),
                        SimplePat.Typed.WithValue(tp),
                        SimplePat.IsOptional.WithValue(isOptional)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member SimplePat(name: string, tp: string, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        SimplePat.Name.WithValue(name),
                        SimplePat.Typed.WithValue(CommonType.mkLongIdent tp),
                        SimplePat.IsOptional.WithValue(isOptional)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type SimplePatNodeModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<SimplePatNode>, attributes: string list) =
        this.AddScalar(SimplePat.MultipleAttributes.WithValue(attributes))
