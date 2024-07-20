namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ObjExpr =
    let Bindings = Attributes.defineWidgetCollection "Value"

    let Members = Attributes.defineWidgetCollection "Members"

    let Interfaces = Attributes.defineScalar<InterfaceImplNode list> "Interface"

    let Value = Attributes.defineWidget "ExprValue"

    let Name = Attributes.defineWidget "TypeValue"

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            let bindings =
                Widgets.tryGetNodesFromWidgetCollection widget Bindings
                |> ValueOption.defaultValue []

            let members =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            let interfaces =
                Widgets.tryGetScalarValue widget Interfaces |> ValueOption.defaultValue []

            let typeName = Widgets.getNodeFromWidget<Type> widget Name
            let exprValue = Widgets.tryGetNodeFromWidget<Expr> widget Value

            let exprValue =
                match exprValue with
                | ValueNone -> None
                | ValueSome expr -> Some expr

            Expr.ObjExpr(
                ExprObjExprNode(
                    SingleTextNode.leftCurlyBrace,
                    SingleTextNode.``new``,
                    typeName,
                    exprValue,
                    Some(SingleTextNode.``with``),
                    bindings,
                    members,
                    interfaces,
                    SingleTextNode.rightCurlyBrace,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ObjExprBuilders =
    type Ast with

        static member ObjExpr(name: WidgetBuilder<Type>) =
            CollectionBuilder<Expr, BindingNode>(
                ObjExpr.WidgetKey,
                ObjExpr.Bindings,
                AttributesBundle(StackList.empty(), [| ObjExpr.Name.WithValue(name.Compile()) |], Array.empty)
            )

        static member ObjExpr(name: string) =
            Ast.ObjExpr(Ast.EscapeHatch(Type.Create(name)))

        static member ObjExpr(name: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, BindingNode>(
                ObjExpr.WidgetKey,
                ObjExpr.Bindings,
                AttributesBundle(
                    StackList.empty(),
                    [| ObjExpr.Name.WithValue(name.Compile())
                       ObjExpr.Value.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member ObjExpr(name: string, expr: string) =
            Ast.ObjExpr(Ast.EscapeHatch(Type.Create(name)), Ast.ConstantExpr(expr))

type ObjExprModifiers =

    [<Extension>]
    static member inline members(this: WidgetBuilder<Expr>) =
        AttributeCollectionBuilder<Expr, BindingNode>(this, ObjExpr.Members)

type ObjExprYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<Expr, BindingNode>, x: BindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x))
        { Widgets = MutStackArray1.One(widget.Compile()) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<Expr, BindingNode>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ObjExprYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<Expr, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<Expr, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<Expr, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ObjExprYieldExtensions.Yield(this, node)
