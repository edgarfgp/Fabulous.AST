namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    let Value = Attributes.defineWidget "Value"

    let TypeParams = Attributes.defineScalar<string list> "TyparDecls"

    let WidgetKey =
        Widgets.register "ConstPat" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value
            Pattern.Const(value))

    let WidgetUnitKey =
        Widgets.register "UnitPat" (fun _ ->
            Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)))

    let WidgetNullKey =
        Widgets.register "NullPat" (fun _ -> Pattern.Null(SingleTextNode.``null``))

    let WidgetWildKey =
        Widgets.register "WildPat" (fun _ -> Pattern.Wild(SingleTextNode.underscore))

[<AutoOpen>]
module PatternBuilders =
    type Ast with

        static member ConstantPat(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Pattern>(Pattern.WidgetKey, Pattern.Value.WithValue(value.Compile()))

        static member ConstantPat(value: string) = Ast.ConstantPat(Ast.Constant(value))

        static member NullPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetNullKey)

        static member WildPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetWildKey)

        static member UnitPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetUnitKey)

type PatternModifiers =
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<Pattern>, values: string list) =
        this.AddScalar(Pattern.TypeParams.WithValue(values))
