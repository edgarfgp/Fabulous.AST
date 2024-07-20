namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Delegate =

    let Name = Attributes.defineScalar<string> "Name"
    let Parameters = Attributes.defineScalar<Type list> "Parameters"

    let Return = Attributes.defineWidget "ReturnType"

    let WidgetKey =
        Widgets.register "TypeDefnDelegateNode" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let returnType = Widgets.getNodeFromWidget<Type> widget Return
            let parameters = Widgets.tryGetScalarValue widget Parameters

            let parameters =
                match parameters with
                | ValueSome parameters -> parameters |> List.map(fun x -> (x, SingleTextNode.rightArrow))
                | ValueNone -> []

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
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<TypeDefnDelegateNode>(
                Delegate.WidgetKey,
                AttributesBundle(
                    StackList.two(Delegate.Name.WithValue(name), Delegate.Parameters.WithValue(parameters)),
                    [| Delegate.Return.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member Delegate(name: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters, returnType)

        static member Delegate(name: string, parameters: string list, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, parameters |> List.map Ast.LongIdent, returnType)

        static member Delegate(name: string, parameters: string list, returnType: string) =
            Ast.BaseDelegate(name, parameters |> List.map Ast.LongIdent, Ast.LongIdent returnType)

        static member Delegate(name: string, parameters: WidgetBuilder<Type> list, returnType: string) =
            Ast.BaseDelegate(name, parameters, Ast.LongIdent returnType)

        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ parameter ], returnType)

        static member Delegate(name: string, parameter: WidgetBuilder<Type>, returnType: string) =
            Ast.BaseDelegate(name, [ parameter ], Ast.LongIdent returnType)

        static member Delegate(name: string, parameter: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseDelegate(name, [ Ast.LongIdent parameter ], returnType)

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
