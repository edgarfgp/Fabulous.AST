namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

module Expr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "ConstantExpr" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value
            Expr.Constant(value))

    let WidgetNullKey =
        Widgets.register "ExprNull" (fun _ -> Expr.Null(SingleTextNode.``null``))

    let WidgetIndexRangeWildcardKey =
        Widgets.register "ExprIndexRangeWildcard" (fun _ -> Expr.IndexRangeWildcard(SingleTextNode.star))

[<AutoOpen>]
module ExprBuilders =
    type Ast with
        /// <summary>Creates a constant expression.</summary>
        /// <param name="value">The constant value.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     ConstantExpr(String("Hello, world!"))
        /// }
        /// </code>
        static member ConstantExpr(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Expr>(Expr.WidgetKey, Expr.Value.WithValue(value.Compile()))

        /// <summary>Creates a constant expression from a string value.</summary>
        /// <param name="value">The constant value as a string.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     ConstantExpr("42")
        /// }
        /// </code>
        static member ConstantExpr(value: string) = Ast.ConstantExpr(Ast.Constant(value))

        /// <summary>Creates a null expression.</summary>
        /// <code lang="fsharp">
        /// Oak() {
        ///     NullExpr()
        /// }
        /// </code>
        static member NullExpr() = WidgetBuilder<Expr>(Expr.WidgetNullKey)

        /// <summary>Creates a unit expression ().</summary>
        /// <code lang="fsharp">
        /// Oak() {
        ///     UnitExpr()
        /// }
        /// </code>
        static member UnitExpr() = Ast.ConstantExpr(Ast.ConstantUnit())

        /// <summary>Creates an index range wildcard expression (*).</summary>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         IndexRangeWildcardExpr()
        ///     }
        /// }
        /// </code>
        static member IndexRangeWildcardExpr() =
            WidgetBuilder<Expr>(Expr.WidgetIndexRangeWildcardKey)

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

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<Expr> seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun widget ->
                let node = Gen.mkOak widget
                let moduleDecl = ModuleDecl.DeclExpr node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }
