namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let Type = Attributes.defineScalar<Type option> "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Helpers.getNodeFromWidget<Pattern> widget Value
            let typeValue = Helpers.tryGetScalarValue widget Type

            let typeValue =
                match typeValue with
                | ValueSome t -> t
                | ValueNone -> None

            Pattern.Parameter(PatParameterNode(None, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: WidgetBuilder<Pattern>, pType: Type option) =
            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                AttributesBundle(
                    StackList.one(Parameter.Type.WithValue(pType)),
                    ValueSome [| Parameter.Value.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: Type) = Ast.BaseParameter(name, Some pType)

        static member ParameterPat(name: WidgetBuilder<Pattern>) = Ast.BaseParameter(name, None)

        static member ParameterPat(name: string, pType: Type) =
            Ast.BaseParameter(
                Ast.EscapeHatch(Pattern.Named(PatNamedNode(None, SingleTextNode.Create(name), Range.Zero))),
                Some pType
            )

        static member ParameterPat(name: string) =
            Ast.BaseParameter(
                Ast.EscapeHatch(Pattern.Named(PatNamedNode(None, SingleTextNode.Create(name), Range.Zero))),
                None
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.BaseParameter(name, Some(CommonType.mkLongIdent pType))

        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, CommonType.mkLongIdent pType)
