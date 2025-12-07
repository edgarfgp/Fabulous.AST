namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TraitCall =
    let Value = Attributes.defineWidget "Value"

    let Typed = Attributes.defineWidget "TypedValue"

    let MemberDef = Attributes.defineScalar<MemberDefn> "Member"

    let WidgetKey =
        Widgets.register "TraitCall" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Value
            let typed = Widgets.getNodeFromWidget<Type> widget Typed
            let memberDef = Widgets.getScalarValue widget MemberDef
            Expr.TraitCall(ExprTraitCallNode(typed, memberDef, expr, Range.Zero)))

[<AutoOpen>]
module TraitCallBuilders =
    type Ast with

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<InheritConstructor>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.ImplicitInherit

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<InheritConstructor>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<InheritConstructor>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(t, memberDef, Ast.ConstantExpr(expr))

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<InheritConstructor>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.Constant expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnInheritNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.Inherit

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnInheritNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnInheritNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnInheritNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<FieldNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.ValField

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<FieldNode>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<FieldNode>, expr: WidgetBuilder<Constant>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<FieldNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<BindingNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.Member

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<BindingNode>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<BindingNode>, expr: WidgetBuilder<Constant>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<BindingNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<ExternBindingNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.ExternBinding

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<ExternBindingNode>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<ExternBindingNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<ExternBindingNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<ExprSingleNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.DoExpr

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<ExprSingleNode>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<ExprSingleNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<ExprSingleNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<BindingListNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.LetBinding

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<BindingListNode>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<BindingListNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<BindingListNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnExplicitCtorNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.ExplicitCtor

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnExplicitCtorNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnExplicitCtorNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnExplicitCtorNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnInterfaceNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.Interface

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnInterfaceNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnInterfaceNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnInterfaceNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnAutoPropertyNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.AutoProperty

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnAutoPropertyNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnAutoPropertyNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnAutoPropertyNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnAbstractSlotNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.AbstractSlot

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnAbstractSlotNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnAbstractSlotNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnAbstractSlotNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnPropertyGetSetNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.PropertyGetSet

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnPropertyGetSetNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnPropertyGetSetNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnPropertyGetSetNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefnSigMemberNode>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef |> MemberDefn.SigMember

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnSigMemberNode>, expr: WidgetBuilder<Expr>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr
            (t: string, memberDef: WidgetBuilder<MemberDefnSigMemberNode>, expr: WidgetBuilder<Constant>)
            =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefnSigMemberNode>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr
            (t: WidgetBuilder<Type>, memberDef: WidgetBuilder<MemberDefn>, expr: WidgetBuilder<Expr>)
            =
            let memberDef = Gen.mkOak memberDef

            WidgetBuilder<Expr>(
                TraitCall.WidgetKey,
                AttributesBundle(
                    StackList.one(TraitCall.MemberDef.WithValue(memberDef)),
                    [| TraitCall.Typed.WithValue(t.Compile())
                       TraitCall.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefn>, expr: WidgetBuilder<Expr>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefn>, expr: WidgetBuilder<Constant>) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)

        static member TraitCallExpr(t: string, memberDef: WidgetBuilder<MemberDefn>, expr: string) =
            Ast.TraitCallExpr(Ast.EscapeHatch(Type.Create(t)), memberDef, Ast.ConstantExpr expr)
