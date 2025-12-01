namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let TypeVal = Attributes.defineWidget "Type"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getNodeFromWidget<Pattern> widget Value

            let typeValue =
                Widgets.tryGetNodeFromWidget widget TypeVal
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            Pattern.Parameter(PatParameterNode(attributes, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type> voption) =
            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| match pType with
                       | ValueSome pType ->
                           Parameter.Value.WithValue(name.Compile())
                           Parameter.TypeVal.WithValue(pType.Compile())
                       | ValueNone -> Parameter.Value.WithValue(name.Compile()) |],
                    Array.empty
                )
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(name, ValueSome(pType))

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        static member ParameterPat(name: WidgetBuilder<Constant>, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        static member ParameterPat(name: WidgetBuilder<Constant>, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        static member ParameterPat(name: WidgetBuilder<Pattern>) = Ast.BaseParameter(name, ValueNone)

        static member ParameterPat(name: WidgetBuilder<Constant>) = Ast.ParameterPat(Ast.ConstantPat(name))

        static member ParameterPat(name: string) = Ast.ParameterPat(Ast.Constant(name))

type ParameterModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<Pattern>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(
            Parameter.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<Pattern>, attribute: WidgetBuilder<AttributeNode>) =
        ParameterModifiers.attributes(this, [ attribute ])
