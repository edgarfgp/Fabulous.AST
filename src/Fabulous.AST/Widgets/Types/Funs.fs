namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open type Fantomas.Core.SyntaxOak.Type

module TypeFuns =

    let Return = Attributes.defineWidget "Return"
    let Parameters = Attributes.defineScalar<Type list> "Parameters"

    let WidgetKey =
        Widgets.register "TypeFuns" (fun widget ->
            let returnType = Widgets.getNodeFromWidget<Type> widget Return
            let parameters = Widgets.tryGetScalarValue widget Parameters

            let members =
                match parameters with
                | ValueSome parameters -> parameters |> List.map(fun x -> (x, SingleTextNode.rightArrow))
                | ValueNone -> []

            Type.Funs(TypeFunsNode(members, returnType, Range.Zero)))

[<AutoOpen>]
module FunsBuilders =
    type Ast with

        static member Funs(returnType: WidgetBuilder<Type>, parameters: WidgetBuilder<Type> list) =
            WidgetBuilder<Type>(
                TypeFuns.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeFuns.Parameters.WithValue(parameters |> List.map Gen.mkOak)),
                    [| TypeFuns.Return.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member Funs(returnType: string, parameters: WidgetBuilder<Type> list) =
            Ast.Funs(Ast.LongIdent returnType, parameters)

        static member Funs(returnType: WidgetBuilder<Type>, parameter: WidgetBuilder<Type>) =
            Ast.Funs(returnType, [ parameter ])

        static member Funs(returnType: string, parameter: WidgetBuilder<Type>) =
            Ast.Funs(Ast.LongIdent returnType, [ parameter ])

        static member Funs(returnType: string, parameter: string) =
            Ast.Funs(Ast.LongIdent returnType, [ Ast.LongIdent parameter ])

        static member Funs(returnType: WidgetBuilder<Type>, parameters: string list) =
            Ast.Funs(returnType, parameters |> List.map Ast.LongIdent)

        static member Funs(returnType: string, parameters: string list) =
            Ast.Funs(Ast.LongIdent returnType, parameters |> List.map Ast.LongIdent)
