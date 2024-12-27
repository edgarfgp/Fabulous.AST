namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Delegate =

    let Name = Attributes.defineScalar<string> "Name"
    let Parameters = Attributes.defineScalar<Type list> "Parameters"

    let Return = Attributes.defineWidget "ReturnType"

    let WidgetKey =
        Widgets.register "TypeDefnDelegateNode" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let returnType = Widgets.getNodeFromWidget<Type> widget Return

            let parameters = Widgets.getScalarValue widget Parameters

            let parameters =
                parameters
                |> List.mapi(fun i t ->
                    if i = List.length parameters - 1 then
                        (t, SingleTextNode.arrow)
                    else
                        (t, SingleTextNode.star))

            TypeDefnDelegateNode(
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                SingleTextNode.``delegate``,
                TypeFunsNode(parameters, returnType, Range.Zero),
                Range.Zero
            ))

[<AutoOpen>]
module DelegateBuilders =
    type Ast with
        static member private BaseDelegate
            (name: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<TypeDefnDelegateNode>(
                Delegate.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Delegate.Name.WithValue(name),
                        Delegate.Parameters.WithValue(parameters |> List.map Gen.mkOak)
                    ),
                    [| Delegate.Return.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ Paren(Tuple([ Int(); Int() ])); Paren(Tuple([ Int(); Int() ])) ], Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters, returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ "int" ; "int" ], Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: string list, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters |> List.map Ast.LongIdent, returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ "int" ; "int" ], "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: string list, returnType: string) =
            Ast.BaseDelegate(name, parameters |> List.map Ast.LongIdent, Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameters">The parameters of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", [ Int(); Int() ], "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameters: WidgetBuilder<Type> list, returnType: string) =
            Ast.BaseDelegate(name, parameters, Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", Int(), Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ parameter ], returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", Int(), "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: string) =
            Ast.BaseDelegate(name, [ parameter ], Ast.LongIdent returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", "int", Int())
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ Ast.LongIdent parameter ], returnType)

        /// <summary>Create a delegate type definition.</summary>
        /// <param name="name">The name of the delegate.</param>
        /// <param name="parameter">The parameter of the delegate.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Delegate("Delegate", "int", "int")
        ///     }
        /// }
        /// </code>
        static member Delegate(name: string, parameter: string, returnType: string) =
            Ast.BaseDelegate(name, [ Ast.LongIdent parameter ], Ast.LongIdent returnType)

type DelegateYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnDelegateNode) : CollectionContent =
        let typeDefn = TypeDefn.Delegate(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnDelegateNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        DelegateYieldExtensions.Yield(this, node)
