namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ObjExpr =
    let Bindings = Attributes.defineWidgetCollection "Value"

    let Members = Attributes.defineWidgetCollection "Members"

    let Value = Attributes.defineWidget "ExprValue"

    let Name = Attributes.defineWidget "TypeValue"

    /// The widget an object expression item compiles to: a `Member(...)` is stored as its binding.
    let itemWidget(item: MemberDefn) =
        match item with
        | MemberDefn.Member bindingNode -> Ast.EscapeHatch(bindingNode).Compile()
        | other -> Ast.EscapeHatch(other).Compile()

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            // Both the body and `.members()` hold a mix of bindings, interface implementations
            // and other member definitions; sort them into the node's three lists.
            let items =
                [ yield!
                      Widgets.tryGetNodesFromWidgetCollection<obj> widget Bindings
                      |> ValueOption.defaultValue []
                  yield!
                      Widgets.tryGetNodesFromWidgetCollection<obj> widget Members
                      |> ValueOption.defaultValue [] ]

            let bindings =
                items
                |> List.choose(fun item ->
                    match item with
                    | :? BindingNode as node -> Some node
                    | :? MemberDefn as m ->
                        match m with
                        | MemberDefn.Member node -> Some node
                        | _ -> None
                    | _ -> None)

            // Besides members and interface implementations, object expressions only allow
            // property get/set members; `do`, `let`, `member val` and the rest do not compile.
            let members =
                items
                |> List.choose(fun item ->
                    match item with
                    | :? MemberDefn as m ->
                        match m with
                        | MemberDefn.Member _
                        | MemberDefn.Interface _ -> None
                        | MemberDefn.PropertyGetSet _ -> Some m
                        | other ->
                            invalidArg
                                "item"
                                $"ObjExpr accepts members (Member(...), property get/set) and InterfaceWith(...) blocks. Got: %s{other.GetType().Name}"
                    | _ -> None)

            // An `InterfaceWith(...)` block is an extra interface implementation
            // (`{ new A with member ... interface IB with member ... }`).
            let interfaces =
                items
                |> List.choose(fun item ->
                    match item with
                    | :? InterfaceImplNode as node -> Some node
                    | :? MemberDefn as m ->
                        match m with
                        | MemberDefn.Interface node ->
                            Some(
                                InterfaceImplNode(
                                    node.Interface,
                                    node.Type,
                                    node.With,
                                    [],
                                    node.Members,
                                    Range.range0
                                )
                            )
                        | _ -> None
                    | _ -> None)

            let typeName = Widgets.getNodeFromWidget<Type> widget Name

            let exprValue =
                Widgets.tryGetNodeFromWidget<Expr> widget Value |> ValueOption.toOption

            // `with` belongs to the members of the base type; an object expression that only
            // implements interfaces (`{ new obj() interface IDisposable with ... }`) has none.
            let withNode =
                if List.isEmpty bindings && List.isEmpty members then
                    None
                else
                    Some(SingleTextNode.``with``)

            Expr.ObjExpr(
                ExprObjExprNode(
                    SingleTextNode.leftCurlyBrace,
                    SingleTextNode.``new``,
                    typeName,
                    exprValue,
                    withNode,
                    bindings,
                    members,
                    interfaces,
                    SingleTextNode.rightCurlyBrace,
                    Range.range0
                )
            ))

[<AutoOpen>]
module ObjExprBuilders =
    type Ast with

        /// <summary>
        /// Create an object expression (<c>{ new T with ... }</c>). The body takes the members of
        /// <c>T</c> and, after them, <c>InterfaceWith(...)</c> blocks for any extra interfaces.
        /// Fantomas lays out an object expression with no members of its own and only extra
        /// interfaces in a way the compiler rejects, so give it at least one member, or use the
        /// interface itself as <c>T</c>.
        /// </summary>
        /// <param name="name">The type to create an instance of.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", ObjExpr("System.IDisposable") {
        ///             Member("_.Dispose", UnitPat(), ConstantExpr(ConstantUnit()))
        ///             InterfaceWith("System.IFormattable") {
        ///                 Member("_.ToString(format: string, provider: System.IFormatProvider)", ConstantExpr(String("x")))
        ///             }
        ///         })
        ///     }
        /// }
        /// </code>
        static member ObjExpr(name: WidgetBuilder<Type>) =
            CollectionBuilder<Expr, BindingNode>(
                ObjExpr.WidgetKey,
                ObjExpr.Bindings,
                ObjExpr.Name.WithValue(name.Compile())
            )

        /// <summary>Create an object expression (<c>{ new T with ... }</c>) for the named type.</summary>
        /// <param name="name">The type to create an instance of.</param>
        static member ObjExpr(name: string) =
            Ast.ObjExpr(Ast.EscapeHatch(Type.Create(name)))

        /// <summary>
        /// Create an object expression that calls a base class constructor (<c>{ new T(args) with ... }</c>).
        /// </summary>
        /// <param name="name">The base class.</param>
        /// <param name="expr">The constructor arguments, such as <c>ConstantExpr(ConstantUnit())</c>.</param>
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

        /// <summary>
        /// Create an object expression that calls a base class constructor (<c>{ new T(args) with ... }</c>).
        /// </summary>
        /// <param name="name">The base class.</param>
        /// <param name="expr">The constructor arguments.</param>
        static member ObjExpr(name: string, expr: string) =
            Ast.ObjExpr(Ast.EscapeHatch(Type.Create(name)), Ast.ConstantExpr(expr))

type ObjExprModifiers =

    /// <summary>
    /// Adds more members or <c>InterfaceWith(...)</c> blocks to an object expression, the same as
    /// putting them in its body.
    /// </summary>
    [<Extension>]
    static member inline members(this: WidgetBuilder<Expr>) =
        AttributeCollectionBuilder<Expr, MemberDefn>(this, ObjExpr.Members)

type ObjExprYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<Expr, BindingNode>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(Gen.mkOak x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<Expr, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<Expr, MemberDefn>, x: WidgetBuilder<MemberDefn>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(ObjExpr.itemWidget(Gen.mkOak x)) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<Expr, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(Gen.mkOak x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<Expr, BindingNode>, x: WidgetBuilder<MemberDefn>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(ObjExpr.itemWidget(Gen.mkOak x)) }
