namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineScalar<StringOrWidget<Pattern>> "Value"
    let Type = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            let value =
                match value with
                | StringOrWidget.StringExpr name ->
                    let name = StringParsing.normalizeIdentifierQuotes name
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create name, Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let typeValue = Widgets.tryGetNodeFromWidget<Type> widget Type

            let typeValue =
                match typeValue with
                | ValueSome t -> Some t
                | ValueNone -> None

            Pattern.Parameter(PatParameterNode(None, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: StringOrWidget<Pattern>, pType: WidgetBuilder<Type> voption) =
            let widgets =
                match pType with
                | ValueSome pType -> [| Parameter.Type.WithValue(pType.Compile()) |]
                | ValueNone -> Array.empty

            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                AttributesBundle(StackList.one(Parameter.Value.WithValue(name)), widgets, Array.empty)
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(StringOrWidget.WidgetExpr(Gen.mkOak name), ValueSome pType)

        static member ParameterPat(name: WidgetBuilder<Pattern>) =
            Ast.BaseParameter(StringOrWidget.WidgetExpr(Gen.mkOak name), ValueNone)

        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(StringOrWidget.StringExpr(Unquoted(name)), ValueSome pType)

        static member ParameterPat(name: string) =
            Ast.BaseParameter(StringOrWidget.StringExpr(Unquoted(name)), ValueNone)

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.BaseParameter(StringOrWidget.WidgetExpr(Gen.mkOak name), ValueSome(Ast.LongIdent pType))

        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, Ast.LongIdent pType)
