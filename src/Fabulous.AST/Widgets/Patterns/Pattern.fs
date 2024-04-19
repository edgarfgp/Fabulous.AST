namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    let Value = Attributes.defineScalar<StringOrWidget<Constant>> "Value"

    let NullValue = Attributes.defineScalar<SingleTextNode> "Null"

    let WildValue = Attributes.defineScalar<SingleTextNode> "Wild"

    let TypeParams = Attributes.defineScalar<string list> "TyparDecls"

    let WidgetKey =
        Widgets.register "ConstPat" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            let value =
                match value with
                | StringOrWidget.StringExpr value ->
                    Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                | StringOrWidget.WidgetExpr constant -> constant

            Pattern.Const(value))

    let WidgetUnitKey =
        Widgets.register "UnitPat" (fun widget ->
            Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)))

    let WidgetNullKey =
        Widgets.register "NullPat" (fun widget ->
            let value = Widgets.getScalarValue widget NullValue
            Pattern.Null(value))

    let WidgetWildKey =
        Widgets.register "WildPat" (fun widget ->
            let value = Widgets.getScalarValue widget WildValue
            Pattern.Wild(value))

[<AutoOpen>]
module PatternBuilders =
    type Ast with

        static member ConstantPat(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Pattern>(
                Pattern.WidgetKey,
                AttributesBundle(
                    StackList.one(Pattern.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(value)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member ConstantPat(value: StringVariant) =
            WidgetBuilder<Pattern>(
                Pattern.WidgetKey,
                AttributesBundle(
                    StackList.one(Pattern.Value.WithValue(StringOrWidget.StringExpr value)),
                    Array.empty,
                    Array.empty
                )
            )

        static member NullPat() =
            WidgetBuilder<Pattern>(
                Pattern.WidgetNullKey,
                AttributesBundle(
                    StackList.one(Pattern.NullValue.WithValue(SingleTextNode.``null``)),
                    Array.empty,
                    Array.empty
                )
            )

        static member WildPat() =
            WidgetBuilder<Pattern>(
                Pattern.WidgetWildKey,
                AttributesBundle(
                    StackList.one(Pattern.WildValue.WithValue(SingleTextNode.underscore)),
                    Array.empty,
                    Array.empty
                )
            )

        static member UnitPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetUnitKey, AttributesBundle(StackList.empty(), Array.empty, Array.empty))

type PatternModifiers =
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<Pattern>, values: string list) =
        this.AddScalar(Pattern.TypeParams.WithValue(values))
