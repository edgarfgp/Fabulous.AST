namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineScalar<StringOrWidget<Pattern>> "Value"
    let TypeVal = Attributes.defineScalar<StringOrWidget<Type>> "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            let value =
                match value with
                | StringOrWidget.StringExpr name ->
                    let name = StringParsing.normalizeIdentifierQuotes name
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create name, Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let typeValue = Widgets.tryGetScalarValue widget TypeVal

            let typeValue =
                match typeValue with
                | ValueSome typeValue ->
                    match typeValue with
                    | StringOrWidget.StringExpr value ->
                        Some(
                            Type.LongIdent(
                                IdentListNode(
                                    [ IdentifierOrDot.Ident(SingleTextNode.Create(value.Normalize())) ],
                                    Range.Zero
                                )
                            )
                        )
                    | StringOrWidget.WidgetExpr widget -> Some widget
                | ValueNone -> None

            Pattern.Parameter(PatParameterNode(None, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: StringOrWidget<Pattern>, pType: StringOrWidget<Type> voption) =
            let scalars =
                match pType with
                | ValueSome pType -> StackList.two(Parameter.Value.WithValue(name), Parameter.TypeVal.WithValue(pType))
                | ValueNone -> StackList.one(Parameter.Value.WithValue(name))

            WidgetBuilder<Pattern>(Parameter.WidgetKey, AttributesBundle(scalars, Array.empty, Array.empty))

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                ValueSome(StringOrWidget.WidgetExpr(Gen.mkOak pType))
            )

        static member ParameterPat(name: WidgetBuilder<Pattern>) =
            Ast.BaseParameter(StringOrWidget.WidgetExpr(Gen.mkOak name), ValueNone)

        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(
                StringOrWidget.StringExpr(Unquoted(name)),
                ValueSome(StringOrWidget.WidgetExpr(Gen.mkOak pType))
            )

        static member ParameterPat(name: string) =
            Ast.BaseParameter(StringOrWidget.StringExpr(Unquoted(name)), ValueNone)

        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.BaseParameter(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                ValueSome(StringOrWidget.StringExpr(Unquoted(pType)))
            )

        static member ParameterPat(name: string, pType: string) =
            Ast.BaseParameter(
                StringOrWidget.StringExpr(Unquoted name),
                ValueSome(StringOrWidget.StringExpr(Unquoted pType))
            )
