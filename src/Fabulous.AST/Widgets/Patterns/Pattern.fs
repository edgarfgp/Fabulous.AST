namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Pattern =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let value = Helpers.getNodeFromWidget<Pattern> widget Value
            value)

[<AutoOpen>]
module PatternBuilders =
    type Ast with

        static member private BasePattern(value: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Pattern.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Pattern.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member UnitPat() =
            Ast.BasePattern(
                Ast.EscapeHatch(
                    Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
                )
            )

        static member NullPat() =
            Ast.BasePattern(Ast.EscapeHatch(Pattern.Null(SingleTextNode.``null``)))

        static member WildPat() =
            Ast.BasePattern(Ast.EscapeHatch(Pattern.Wild(SingleTextNode.underscore)))

[<Extension>]
type ParametersYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Pattern>, x: WidgetBuilder<Pattern>) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
