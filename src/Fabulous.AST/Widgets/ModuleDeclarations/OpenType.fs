namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module OpenType =
    let Target = Attributes.defineScalar<StringOrWidget<Type>> "Target"

    let WidgetKey =
        Widgets.register "OpenType" (fun widget ->
            let target = Widgets.getScalarValue widget Target

            let target =
                match target with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            OpenTargetNode(target, Range.Zero))

[<AutoOpen>]
module OpenTypeBuilders =
    type Ast with

        static member OpenType(name: string) =
            WidgetBuilder<OpenTargetNode>(
                OpenType.WidgetKey,
                AttributesBundle(
                    StackList.one(OpenType.Target.WithValue(StringOrWidget.StringExpr(Unquoted name))),
                    Array.empty,
                    Array.empty
                )
            )

        static member OpenType(name: WidgetBuilder<Type>) =
            WidgetBuilder<OpenTargetNode>(
                OpenType.WidgetKey,
                AttributesBundle(
                    StackList.one(OpenType.Target.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak name))),
                    Array.empty,
                    Array.empty
                )
            )

type OpenTypeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<OpenTargetNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let openList = OpenListNode([ Open.Target node ])
        let moduleDecl = ModuleDecl.OpenList openList
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
