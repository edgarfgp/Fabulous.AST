namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeConstraint =
    let Single = Attributes.defineScalar<struct (string * string)> "Rank"

    let WidgetSingleKey =
        Widgets.register "TypeConstraintSingle" (fun widget ->
            let struct (typar, kind) = Widgets.getScalarValue widget Single

            TypeConstraint.Single(
                TypeConstraintSingleNode(SingleTextNode.Create(typar), SingleTextNode.Create(kind), Range.Zero)
            ))

    let DefaultsToType =
        Attributes.defineScalar<struct (string * string * Type)> "DefaultsToType"

    let WidgetDefaultsToTypeKey =
        Widgets.register "DefaultsToType" (fun widget ->
            let struct (def, typar, tp) = Widgets.getScalarValue widget DefaultsToType

            TypeConstraint.DefaultsToType(
                TypeConstraintDefaultsToTypeNode(
                    SingleTextNode.Create(def),
                    SingleTextNode.Create(typar),
                    tp,
                    Range.Zero
                )
            ))

    let SubtypeOfType = Attributes.defineScalar<struct (string * Type)> "SubtypeOfType"

    let WidgetSubtypeOfTypeKey =
        Widgets.register "SubtypeOfType" (fun widget ->
            let struct (typar, tp) = Widgets.getScalarValue widget SubtypeOfType
            TypeConstraint.SubtypeOfType(TypeConstraintSubtypeOfTypeNode(SingleTextNode.Create(typar), tp, Range.Zero)))

    let SupportsMember =
        Attributes.defineScalar<struct (Type * MemberDefn)> "SupportsMember"

    let WidgetSupportsMemberKey =
        Widgets.register "SupportsMember" (fun widget ->
            let struct (tp, memberDefn) = Widgets.getScalarValue widget SupportsMember
            TypeConstraint.SupportsMember(TypeConstraintSupportsMemberNode(tp, memberDefn, Range.Zero)))

    let EnumOrDelegate =
        Attributes.defineScalar<struct (string * string * Type list)> "EnumOrDelegate"

    let WidgetEnumOrDelegateKey =
        Widgets.register "EnumOrDelegate" (fun widget ->
            let struct (tp, verb, ts) = Widgets.getScalarValue widget EnumOrDelegate

            TypeConstraint.EnumOrDelegate(
                TypeConstraintEnumOrDelegateNode(SingleTextNode.Create(tp), verb, ts, Range.Zero)
            ))

    let WhereSelfConstrained = Attributes.defineWidget "WhereSelfConstrained"

    let WidgetKeyWhereSelfConstrained =
        Widgets.register "TypeConstraintWhereSelfConstrained" (fun widget ->
            let tp = Widgets.getNodeFromWidget<Type> widget WhereSelfConstrained
            TypeConstraint.WhereSelfConstrained(tp))

[<AutoOpen>]
module TypeConstraintBuilders =
    type Ast with
        static member ConstraintSingle(typar: string, kind: string) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSingleKey,
                TypeConstraint.Single.WithValue((typar, kind))
            )

        static member DefaultsToType(def: string, typar: string, tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetDefaultsToTypeKey,
                TypeConstraint.DefaultsToType.WithValue((def, typar, Gen.mkOak tp))
            )

        static member DefaultsToType(def: string, typar: string, tp: string) =
            Ast.DefaultsToType(def, typar, Ast.LongIdent(tp))

        static member SubtypeOfType(typar: string, tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSubtypeOfTypeKey,
                TypeConstraint.SubtypeOfType.WithValue((typar, Gen.mkOak tp))
            )

        static member SubtypeOfType(typar: string, tp: string) =
            Ast.SubtypeOfType(typar, Ast.LongIdent(tp))

        static member EnumOrDelegate(tp: string, verb: string, ts: WidgetBuilder<Type> list) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetEnumOrDelegateKey,
                TypeConstraint.EnumOrDelegate.WithValue((tp, verb, List.map Gen.mkOak ts))
            )

        static member EnumOrDelegate(tp: string, verb: string, ts: WidgetBuilder<Type>) =
            Ast.EnumOrDelegate(tp, verb, [ ts ])

        static member EnumOrDelegate(tp: string, verb: string, ts: string list) =
            let ts = ts |> List.map Ast.LongIdent
            Ast.EnumOrDelegate(tp, verb, ts)

        static member EnumOrDelegate(tp: string, verb: string, ts: string) = Ast.EnumOrDelegate(tp, verb, [ ts ])

        static member WhereSelfConstrained(tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetKeyWhereSelfConstrained,
                TypeConstraint.WhereSelfConstrained.WithValue(tp.Compile())
            )

        static member WhereSelfConstrained(tp: string) =
            Ast.WhereSelfConstrained(Ast.LongIdent tp)

        static member private BaseSupportsMember(tp: WidgetBuilder<Type>, memberDefn: MemberDefn) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSupportsMemberKey,
                TypeConstraint.SupportsMember.WithValue((Gen.mkOak tp, memberDefn))
            )

        static member SupportsMember(tp: WidgetBuilder<Type>, memberDefn: WidgetBuilder<MemberDefnSigMemberNode>) =
            Ast.BaseSupportsMember(tp, MemberDefn.SigMember(Gen.mkOak memberDefn))

        static member SupportsMember(tp: string, memberDefn: WidgetBuilder<MemberDefnSigMemberNode>) =
            Ast.BaseSupportsMember(Ast.LongIdent(tp), MemberDefn.SigMember(Gen.mkOak memberDefn))
