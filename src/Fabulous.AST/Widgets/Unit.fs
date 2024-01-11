namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Unit =
    let WidgetKey =
        Widgets.register "Unit" (fun _ ->
            UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))

[<AutoOpen>]
module UnitBuilders =
    type Ast with

        static member inline Unit() =
            WidgetBuilder<UnitNode>(Unit.WidgetKey, AttributesBundle(StackList.empty(), ValueNone, ValueNone))


[<Extension>]
type UnitYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<UnitNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.DeclExpr(Expr.Constant(Constant.Unit(node)))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
