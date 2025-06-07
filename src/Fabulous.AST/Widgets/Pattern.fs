namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    let Value = Attributes.defineWidget "Value"

    let TypeParams = Attributes.defineScalar<string seq> "TyparDecls"

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

        /// <summary>Creates a constant pattern.</summary>
        /// <param name="value">The constant value.</param>
        static member ConstantPat(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Pattern>(Pattern.WidgetKey, Pattern.Value.WithValue(value.Compile()))

        /// <summary>Creates a constant pattern from a string value.</summary>
        /// <param name="value">The constant value as a string.</param>
        static member ConstantPat(value: string) = Ast.ConstantPat(Ast.Constant(value))

        /// <summary>Creates a null pattern.</summary>
        static member NullPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetNullKey)

        /// <summary>Creates a wildcard pattern _.</summary>
        static member WildPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetWildKey)

        /// <summary>Creates a unit pattern ().</summary>
        static member UnitPat() =
            WidgetBuilder<Pattern>(Pattern.WidgetUnitKey)

type PatternModifiers =
    /// <summary>Sets type parameters for the pattern.</summary>
    /// <param name="this">Current pattern widget.</param>
    /// <param name="values">List of type parameter names.</param>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<Pattern>, values: string seq) =
        this.AddScalar(Pattern.TypeParams.WithValue(values))
