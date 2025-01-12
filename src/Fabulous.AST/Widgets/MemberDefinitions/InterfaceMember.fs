namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module InterfaceMember =
    let TypeValue = Attributes.defineWidget "Type"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "InterfaceMember" (fun widget ->
            let typeVal = Widgets.getNodeFromWidget widget TypeValue

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let withNode =
                if members.IsEmpty then
                    None
                else
                    Some(SingleTextNode.``with``)

            MemberDefnInterfaceNode(SingleTextNode.``interface``, typeVal, withNode, members, Range.Zero))

[<AutoOpen>]
module InterfaceMemberBuilders =
    type Ast with

        /// <summary>
        /// Create an interface member with the given name.
        /// </summary>
        /// <param name="value">The name of the interface.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", ParenPat(ParameterPat("name"))) {
        ///            InterfaceWith(LongIdent("IPrintable")) {
        ///                Member("this.Print", AppExpr("printfn", [ String("%s"); Constant("name") ]))
        ///            }
        ///        }
        /// }
        /// </code>
        static member InterfaceWith(value: WidgetBuilder<Type>) =
            CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>(
                InterfaceMember.WidgetKey,
                InterfaceMember.Members,
                InterfaceMember.TypeValue.WithValue(value.Compile())
            )

        /// <summary>
        /// Create an interface member with the given name.
        /// </summary>
        /// <param name="name">The name of the interface.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", ParenPat(ParameterPat("name"))) {
        ///             InterfaceWith("IPrintable") {
        ///                 Member("this.Print", AppExpr("printfn", [ String("%s"); Constant("name") ]))
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        static member InterfaceWith(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.InterfaceWith(Ast.LongIdent name)

type InterfaceMemberYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x))
        { Widgets = MutStackArray1.One(widget.Compile()) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        InterfaceMemberYieldExtensions.Yield(this, node)
