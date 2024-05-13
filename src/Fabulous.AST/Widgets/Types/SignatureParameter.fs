namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module SignatureParameter =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let TypedValue = Attributes.defineWidget "TypedValue"

    let WidgetKey =
        Widgets.register "SignatureParameter" (fun widget ->
            let identifier =
                Widgets.tryGetScalarValue widget Identifier
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(x)))
                |> ValueOption.defaultValue None

            let value = Widgets.getNodeFromWidget<Type> widget TypedValue

            Type.SignatureParameter(TypeSignatureParameterNode(None, identifier, value, Range.Zero)))

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
