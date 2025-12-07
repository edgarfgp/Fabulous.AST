namespace Fabulous.AST

open Fabulous.AST
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
        Attributes.defineScalar<struct (string * string * Type seq)> "EnumOrDelegate"

    let WidgetEnumOrDelegateKey =
        Widgets.register "EnumOrDelegate" (fun widget ->
            let struct (tp, verb, ts) = Widgets.getScalarValue widget EnumOrDelegate

            TypeConstraint.EnumOrDelegate(
                TypeConstraintEnumOrDelegateNode(SingleTextNode.Create(tp), verb, List.ofSeq ts, Range.Zero)
            ))

    let WhereSelfConstrained = Attributes.defineWidget "WhereSelfConstrained"

    let WidgetKeyWhereSelfConstrained =
        Widgets.register "TypeConstraintWhereSelfConstrained" (fun widget ->
            let tp = Widgets.getNodeFromWidget<Type> widget WhereSelfConstrained
            TypeConstraint.WhereSelfConstrained(tp))

    let ConstraintWhereNotSupportsNull =
        Attributes.defineScalar<string> "ConstraintWhereNotSupportsNull"

    let WidgetKeyWhereNotSupportsNull =
        Widgets.register "TypeConstraintWhereNotSupportsNull" (fun widget ->
            let typar = Widgets.getScalarValue widget ConstraintWhereNotSupportsNull

            TypeConstraint.WhereNotSupportsNull(
                TypeConstraintWhereNotSupportsNull(
                    SingleTextNode.Create(typar),
                    SingleTextNode.colon,
                    SingleTextNode.not,
                    SingleTextNode.``null``,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeConstraintBuilders =
    type Ast with
        static member ConstraintSingle(typar: string, kind: string) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSingleKey,
                TypeConstraint.Single.WithValue((typar, kind))
            )

        static member ConstraintNotStruct(typar: string) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSingleKey,
                TypeConstraint.Single.WithValue((typar, "not struct"))
            )

        static member DefaultsTo(def: string, typar: string, tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetDefaultsToTypeKey,
                TypeConstraint.DefaultsToType.WithValue((def, typar, Gen.mkOak tp))
            )

        static member DefaultsTo(def: string, typar: string, tp: string) =
            Ast.DefaultsTo(def, typar, Ast.LongIdent(tp))

        static member SubtypeOf(typar: string, tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSubtypeOfTypeKey,
                TypeConstraint.SubtypeOfType.WithValue((typar, Gen.mkOak tp))
            )

        static member SubtypeOf(typar: string, tp: string) = Ast.SubtypeOf(typar, Ast.LongIdent(tp))

        static member EnumOrDelegate(tp: string, verb: string, ts: WidgetBuilder<Type> seq) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetEnumOrDelegateKey,
                TypeConstraint.EnumOrDelegate.WithValue((tp, verb, List.map Gen.mkOak (List.ofSeq ts)))
            )

        static member EnumOrDelegate(tp: string, verb: string, ts: WidgetBuilder<Type>) =
            Ast.EnumOrDelegate(tp, verb, [ ts ])

        static member EnumOrDelegate(tp: string, verb: string, ts: string seq) =
            let ts = ts |> Seq.map Ast.LongIdent
            Ast.EnumOrDelegate(tp, verb, ts)

        static member EnumOrDelegate(tp: string, verb: string, ts: string) = Ast.EnumOrDelegate(tp, verb, [ ts ])

        static member WhereSelf(tp: WidgetBuilder<Type>) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetKeyWhereSelfConstrained,
                TypeConstraint.WhereSelfConstrained.WithValue(tp.Compile())
            )

        static member WhereSelf(tp: string) = Ast.WhereSelf(Ast.LongIdent tp)

        static member WhereNotSupportsNull(typar: string) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetKeyWhereNotSupportsNull,
                TypeConstraint.ConstraintWhereNotSupportsNull.WithValue(typar)
            )

        static member private BaseSupports(tp: WidgetBuilder<Type>, memberDefn: MemberDefn) =
            WidgetBuilder<TypeConstraint>(
                TypeConstraint.WidgetSupportsMemberKey,
                TypeConstraint.SupportsMember.WithValue((Gen.mkOak tp, memberDefn))
            )

        static member Supports(tp: WidgetBuilder<Type>, memberDefn: WidgetBuilder<MemberDefnSigMemberNode>) =
            Ast.BaseSupports(tp, MemberDefn.SigMember(Gen.mkOak memberDefn))

        static member Supports(tp: string, memberDefn: WidgetBuilder<MemberDefnSigMemberNode>) =
            Ast.BaseSupports(Ast.LongIdent(tp), MemberDefn.SigMember(Gen.mkOak memberDefn))

        static member Supports(tp: WidgetBuilder<Type>, memberDefn: WidgetBuilder<MemberDefn>) =
            Ast.BaseSupports(tp, Gen.mkOak memberDefn)

        static member Supports(tp: string, memberDefn: WidgetBuilder<MemberDefn>) =
            Ast.BaseSupports(Ast.LongIdent(tp), Gen.mkOak memberDefn)
