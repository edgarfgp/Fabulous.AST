namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Expr =
    let Value = Attributes.defineScalar<StringOrWidget<Constant>> "Value"
    let HasQuotes = Attributes.defineScalar<bool> "HasQuotes"

    let WidgetKey =
        Widgets.register "Expr" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            let hasQuotes =
                Widgets.tryGetScalarValue widget HasQuotes |> ValueOption.defaultValue true

            let value =
                match value with
                | StringOrWidget.StringExpr value ->
                    Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes)))
                | StringOrWidget.WidgetExpr constant -> constant

            Expr.Constant(value))

    let WidgetNullKey =
        Widgets.register "ExprNull" (fun _ -> Expr.Null(SingleTextNode.``null``))

[<AutoOpen>]
module ExprBuilders =
    type Ast with
        static member ConstantExpr(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Expr>(
                Expr.WidgetKey,
                AttributesBundle(
                    StackList.one(Expr.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(value)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member ConstantExpr(value: string) =
            WidgetBuilder<Expr>(
                Expr.WidgetKey,
                AttributesBundle(
                    StackList.one(Expr.Value.WithValue(StringOrWidget.StringExpr value)),
                    Array.empty,
                    Array.empty
                )
            )

        static member NullExpr() =
            WidgetBuilder<Expr>(Expr.WidgetNullKey, AttributesBundle(StackList.empty(), Array.empty, Array.empty))

[<Extension>]
type ExprModifiers =
    [<Extension>]
    static member inline hasQuotes(this: WidgetBuilder<Expr>, value: bool) =
        this.AddScalar(Expr.HasQuotes.WithValue(value))

[<Extension>]
type ExprYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Expr>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.DeclExpr node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
