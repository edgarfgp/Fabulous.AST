namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module BindingList =
    let Bindings = Attributes.defineScalar<BindingNode list> "Type"

    let WidgetKey =
        Widgets.register "LetBindingMember" (fun widget ->
            let bindings = Widgets.getScalarValue widget Bindings
            BindingListNode(bindings, Range.Zero))

[<AutoOpen>]
module LetBindingMemberBuilders =
    type Ast with

        /// <summary>
        /// Define a list of let bindings.
        /// </summary>
        /// <param name="bindings">The list of bindings.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             LetBindings([
        ///                 Value(ConstantPat(Constant "_name"), ConstantExpr(String(""))).toMutable()
        ///                 Value(ConstantPat(Constant "_age"), ConstantExpr(String("0"))).toMutable()
        ///             ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member LetBindings(bindings: WidgetBuilder<BindingNode> list) =
            WidgetBuilder<BindingListNode>(
                BindingList.WidgetKey,
                BindingList.Bindings.WithValue(bindings |> List.map Gen.mkOak)
            )

        /// <summary>
        /// Define a single let binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             LetBinding(Value(ConstantPat(Constant "_name"), ConstantExpr(String(""))).toMutable())
        ///         }
        ///     }
        /// }
        /// </code>
        static member LetBinding(binding: WidgetBuilder<BindingNode>) =
            WidgetBuilder<BindingListNode>(
                BindingList.WidgetKey,
                BindingList.Bindings.WithValue([ binding ] |> List.map Gen.mkOak)
            )
