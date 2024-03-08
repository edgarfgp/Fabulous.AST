namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let Type = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Helpers.getNodeFromWidget<Pattern> widget Value
            let typeValue = Helpers.tryGetNodeFromWidget<Type> widget Type

            let typeValue =
                match typeValue with
                | ValueSome t -> Some t
                | ValueNone -> None

            Pattern.Parameter(PatParameterNode(None, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type> voption) =
            let widgets =
                match pType with
                | ValueSome pType ->
                    [| Parameter.Type.WithValue(pType.Compile())
                       Parameter.Value.WithValue(name.Compile()) |]
                | ValueNone -> [| Parameter.Value.WithValue(name.Compile()) |]

            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome widgets, ValueNone)
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(name, ValueSome pType)

        static member ParameterPat(name: WidgetBuilder<Pattern>) = Ast.BaseParameter(name, ValueNone)

        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(Ast.NamedPat(name), ValueSome pType)

        static member ParameterPat(name: string) =
            Ast.BaseParameter(Ast.NamedPat(name), ValueNone)

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.BaseParameter(name, ValueSome(Ast.LongIdent pType))

        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, Ast.LongIdent pType)
