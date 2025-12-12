namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Inherit =
    let TypeValue = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "InheritMember" (fun widget ->
            let tp = Widgets.getNodeFromWidget widget TypeValue
            let node = MemberDefnInheritNode(SingleTextNode.``inherit``, tp, Range.Zero)
            MemberDefn.Inherit(node))

[<AutoOpen>]
module InheritMemberBuilders =
    type Ast with

        /// <summary>
        /// Create inherit member with a given type.
        /// </summary>
        /// <param name="value">The type of the inherit member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", Constructor(ParameterPat("name", String()))) {
        ///             Inherit(LongIdent("BaseClass()"))
        ///     }
        /// }
        /// </code>
        static member Inherit(value: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefn>(Inherit.WidgetKey, Inherit.TypeValue.WithValue(value.Compile()))

        /// <summary>
        /// Create inherit member with a given type.
        /// </summary>
        /// <param name="value">The type of the inherit member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", Constructor(ParameterPat("name", String()))) {
        ///             Inherit("BaseClass()")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Inherit(value: string) = Ast.Inherit(Ast.LongIdent(value))
