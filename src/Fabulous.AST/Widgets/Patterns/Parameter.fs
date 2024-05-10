namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let TypeVal = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getNodeFromWidget<Pattern> widget Value

            let typeValue =
                Widgets.tryGetNodeFromWidget widget TypeVal
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            Pattern.Parameter(PatParameterNode(None, value, typeValue, Range.Zero)))

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
            Ast.ParameterPat(name, Ast.LongIdent(pType))

        static member ParameterPat(name: WidgetBuilder<Constant>, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        static member ParameterPat(name: WidgetBuilder<Constant>, pType: string) =
            Ast.ParameterPat(name, Ast.LongIdent(pType))

        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, Ast.LongIdent(pType))

        static member ParameterPat(name: WidgetBuilder<Pattern>) = Ast.BaseParameter(name, ValueNone)

        static member ParameterPat(name: WidgetBuilder<Constant>) = Ast.ParameterPat(Ast.ConstantPat(name))

        static member ParameterPat(name: string) = Ast.ParameterPat(Ast.Constant(name))
