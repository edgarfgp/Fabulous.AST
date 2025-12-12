namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SignatureParameter =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let TypedValue = Attributes.defineWidget "TypedValue"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "SignatureParameter_MultipleAttributes"

    let WidgetKey =
        Widgets.register "SignatureParameter" (fun widget ->
            let identifier =
                Widgets.tryGetScalarValue widget Identifier
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(x)))
                |> ValueOption.defaultValue None

            let value = Widgets.getNodeFromWidget<Type> widget TypedValue

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            Type.SignatureParameter(TypeSignatureParameterNode(attributes, identifier, value, Range.Zero)))

[<AutoOpen>]
module SignatureParameterBuilders =
    type Ast with
        static member private BaseSignatureParameter(identifier: string voption, tp: WidgetBuilder<Type>) =
            let scalars =
                match identifier with
                | ValueSome id -> StackList.one(SignatureParameter.Identifier.WithValue(id))
                | ValueNone -> StackList.empty()

            WidgetBuilder<Type>(
                SignatureParameter.WidgetKey,
                AttributesBundle(scalars, [| SignatureParameter.TypedValue.WithValue(tp.Compile()) |], Array.empty)
            )

        static member SignatureParameter(identifier: string, tp: WidgetBuilder<Type>) =
            Ast.BaseSignatureParameter(ValueSome identifier, tp)

        static member SignatureParameter(tp: WidgetBuilder<Type>) =
            Ast.BaseSignatureParameter(ValueNone, tp)

        static member SignatureParameter(identifier: string, tp: string) =
            Ast.SignatureParameter(identifier, Ast.LongIdent tp)

type SignatureParameterModifiers =
    /// <summary>Sets the attributes for the signature parameter.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<Type>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(
            SignatureParameter.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets an attribute for the signature parameter.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<Type>, attribute: WidgetBuilder<AttributeNode>) =
        SignatureParameterModifiers.attributes(this, [ attribute ])
