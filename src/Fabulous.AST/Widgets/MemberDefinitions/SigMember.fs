namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SigMember =
    let Identifier = Attributes.defineWidget "Val"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<ValNode> widget Identifier
            let hasGetterSetter = Widgets.tryGetScalarValue widget HasGetterSetter

            let withGetSetText =
                match hasGetterSetter with
                | ValueSome(true, true) -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))
                | ValueSome(true, false) -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                | ValueSome(false, true) -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                | ValueSome(false, false)
                | ValueNone -> None

            MemberDefnSigMemberNode(identifier, withGetSetText, Range.Zero))

[<AutoOpen>]
module SigMemberBuilders =
    type Ast with

        static member SigMember(identifier: WidgetBuilder<ValNode>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefnSigMemberNode>(
                SigMember.WidgetKey,
                AttributesBundle(
                    StackList.one(SigMember.HasGetterSetter.WithValue(hasGetter, hasSetter)),
                    [| SigMember.Identifier.WithValue(identifier.Compile()) |],
                    Array.empty
                )
            )
