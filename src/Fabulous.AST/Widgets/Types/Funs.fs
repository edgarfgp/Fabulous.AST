namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open type Fantomas.Core.SyntaxOak.Type

module TypeFuns =

    let Return = Attributes.defineWidget "Return"
    let Parameters = Attributes.defineWidgetCollection "Parameters"

    let WidgetKey =
        Widgets.register "TypeFuns" (fun widget ->
            let returnType = Widgets.getNodeFromWidget<Type> widget Return
            let parameters = Widgets.tryGetNodesFromWidgetCollection<Type> widget Parameters

            let members =
                match parameters with
                | Some parameters -> parameters |> List.map(fun x -> (x, SingleTextNode.rightArrow))
                | None -> []

            Type.Funs(TypeFunsNode(members, returnType, Range.Zero)))

[<AutoOpen>]
module FunsBuilders =
    type Ast with

        static member Funs(returnType: WidgetBuilder<Type>) =
            CollectionBuilder<Type, Type>(
                TypeFuns.WidgetKey,
                TypeFuns.Parameters,
                AttributesBundle(StackList.empty(), [| TypeFuns.Return.WithValue(returnType.Compile()) |], Array.empty)
            )

        static member Funs(returnType: string) =
            CollectionBuilder<Type, Type>(
                TypeFuns.WidgetKey,
                TypeFuns.Parameters,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeFuns.Return.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    Array.empty
                )
            )

[<Extension>]
type FunsYieldExtensions =

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<TypeFunsNode, Type>, x: Type) : CollectionContent =
        { Widgets = MutStackArray1.One(Ast.EscapeHatch(x).Compile()) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeFunsNode, Type>, x: WidgetBuilder<Type>)
        : CollectionContent =
        let node = Gen.mkOak x
        FunsYieldExtensions.Yield(this, node)
