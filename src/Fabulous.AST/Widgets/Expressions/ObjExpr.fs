namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ObjExpr =
    let Bindings = Attributes.defineScalar<BindingNode list> "Value"

    let Members = Attributes.defineScalar<MemberDefn list> "Members"

    let Interfaces = Attributes.defineScalar<InterfaceImplNode list> "Interface"

    let ExprValue = Attributes.defineWidget "ExprValue"

    let TypeName = Attributes.defineWidget "TypeValue"

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            let bindings =
                Widgets.tryGetScalarValue widget Bindings |> ValueOption.defaultValue []

            let members =
                Widgets.tryGetScalarValue widget Members |> ValueOption.defaultValue []

            let interfaces =
                Widgets.tryGetScalarValue widget Interfaces |> ValueOption.defaultValue []

            let typeName = Widgets.getNodeFromWidget<Type> widget TypeName
            let exprValue = Widgets.tryGetNodeFromWidget<Expr> widget ExprValue

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

        static member ObjExpr(typeName: WidgetBuilder<Type>) =
            WidgetBuilder<Expr>(
                ObjExpr.WidgetKey,
                AttributesBundle(StackList.empty(), [| ObjExpr.TypeName.WithValue(typeName.Compile()) |], Array.empty)
            )

        static member ObjExpr(typeName: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                ObjExpr.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ObjExpr.TypeName.WithValue(typeName.Compile())
                       ObjExpr.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

[<Extension>]
type ObjExprModifiers =
    [<Extension>]
    static member bindings(this: WidgetBuilder<Expr>, bindings: WidgetBuilder<BindingNode> list) =
        let bindings = bindings |> List.map Gen.mkOak
        this.AddScalar(ObjExpr.Bindings.WithValue(bindings))

    [<Extension>]
    static member members(this: WidgetBuilder<Expr>, members: WidgetBuilder<MemberDefn> list) =
        let members = members |> List.map Gen.mkOak
        this.AddScalar(ObjExpr.Members.WithValue(members))

    [<Extension>]
    static member interfaces(this: WidgetBuilder<Expr>, interfaces: WidgetBuilder<InterfaceImplNode> list) =
        let interfaces = interfaces |> List.map Gen.mkOak
        this.AddScalar(ObjExpr.Interfaces.WithValue(interfaces))
