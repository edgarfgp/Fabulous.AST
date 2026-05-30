namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SigMember =
    let Identifier = Attributes.defineWidget "Val"
    let HasGetter = Attributes.defineScalar<bool * AccessControl> "HasGetter"
    let HasSetter = Attributes.defineScalar<bool * AccessControl> "HasSetter"

    let WidgetKey =
        Widgets.register "SigMember" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<ValNode> widget Identifier
            let hasGetter = Widgets.getScalarValue widget HasGetter
            let hasSetter = Widgets.getScalarValue widget HasSetter
            let withGetSetText = MultipleTextsNode.CreateGetSet(hasGetter, hasSetter)

            let node = MemberDefnSigMemberNode(identifier, withGetSetText, Range.Zero)
            MemberDefn.SigMember(node))

[<AutoOpen>]
module SigMemberBuilders =
    type Ast with

        /// <summary>Creates a signature member.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
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
        static member SigMember
            (
                identifier: WidgetBuilder<ValNode>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            WidgetBuilder<MemberDefn>(
                SigMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        SigMember.HasGetter.WithValue(hasGetter, getterAccessibility),
                        SigMember.HasSetter.WithValue(hasSetter, setterAccessibility)
                    ),
                    [| SigMember.Identifier.WithValue(identifier.Compile()) |],
                    Array.empty
                )
            )
