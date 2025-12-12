namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SigMember =
    let Identifier = Attributes.defineWidget "Val"
    let HasGetter = Attributes.defineScalar<bool> "HasGetter"
    let HasSetter = Attributes.defineScalar<bool> "HasSetter"

    let WidgetKey =
        Widgets.register "SigMember" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<ValNode> widget Identifier
            let hasGetter = Widgets.getScalarValue widget HasGetter
            let hasSetter = Widgets.getScalarValue widget HasSetter

            let withGetSetText =
                match hasGetter, hasSetter with
                | true, true ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``; SingleTextNode.Create("get,"); SingleTextNode.set ]
                        )
                    )
                | true, false -> Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.get ]))
                | false, true -> Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.set ]))
                | false, false -> None

            let node = MemberDefnSigMemberNode(identifier, withGetSetText, Range.Zero)
            MemberDefn.SigMember(node))

[<AutoOpen>]
module SigMemberBuilders =
    type Ast with

        /// <summary>Creates a signature member.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TraitCallExpr(
        ///             Paren(Or("^I", "^R")),
        ///             SigMember(Val([ "static"; "member" ], "Map", Funs(Tuple([ "^I"; "^F" ]), "^R"))),
        ///             TupleExpr([ "source"; "mapping" ])
        ///        )
        ///     }
        /// }
        /// </code>
        static member SigMember(identifier: WidgetBuilder<ValNode>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefn>(
                SigMember.WidgetKey,
                AttributesBundle(
                    StackList.two(SigMember.HasGetter.WithValue(hasGetter), SigMember.HasSetter.WithValue(hasSetter)),
                    [| SigMember.Identifier.WithValue(identifier.Compile()) |],
                    Array.empty
                )
            )
