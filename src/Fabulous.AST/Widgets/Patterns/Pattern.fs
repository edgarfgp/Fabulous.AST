namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    let Value = Attributes.defineScalar<Pattern> "Value"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            value)

[<AutoOpen>]
module PatternBuilders =
    type Ast with

        static member private BasePattern(value: Pattern) =
            WidgetBuilder<Pattern>(
                Pattern.WidgetKey,
                AttributesBundle(StackList.one(Pattern.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member UnitPat() =
            Ast.BasePattern(
                Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
            )

        static member NullPat() =
            Ast.BasePattern(Pattern.Null(SingleTextNode.``null``))

        static member WildPat() =
            Ast.BasePattern(Pattern.Wild(SingleTextNode.underscore))
