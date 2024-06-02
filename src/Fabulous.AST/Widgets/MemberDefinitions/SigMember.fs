namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

module SigMember =
    let Identifier = Attributes.defineWidget "Val"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

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

        static member private BaseSigMember(identifier: WidgetBuilder<ValNode>, hasGetter: bool, hasSetter: bool) =
            WidgetBuilder<MemberDefnSigMemberNode>(
                SigMember.WidgetKey,
                AttributesBundle(
                    StackList.one(SigMember.HasGetterSetter.WithValue(hasGetter, hasSetter)),
                    [| SigMember.Identifier.WithValue(identifier.Compile()) |],
                    Array.empty
                )
            )

        static member SigMember(identifier: WidgetBuilder<ValNode>) =
            Ast.BaseSigMember(identifier, false, false)

        static member SigMemberGet(identifier: WidgetBuilder<ValNode>) =
            Ast.BaseSigMember(identifier, true, false)

        static member SigMemberSet(identifier: WidgetBuilder<ValNode>) =
            Ast.BaseSigMember(identifier, false, true)

        static member SigMemberGetSet(identifier: WidgetBuilder<ValNode>) =
            Ast.BaseSigMember(identifier, true, true)
