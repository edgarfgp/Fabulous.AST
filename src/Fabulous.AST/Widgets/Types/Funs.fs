namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

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

        static member Funs(parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeFuns.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeFuns.Parameters.WithValue(parameters |> List.map Gen.mkOak)),
                    [| TypeFuns.Return.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member Funs(parameters: WidgetBuilder<Type> list, returnType: string) =
            Ast.Funs(parameters, Ast.LongIdent returnType)

        static member Funs(parameter: WidgetBuilder<Type>, returnType: WidgetBuilder<Type>) =
            Ast.Funs([ parameter ], returnType)

        static member Funs(parameter: WidgetBuilder<Type>, returnType: string) =
            Ast.Funs([ parameter ], Ast.LongIdent returnType)

        static member Funs(parameter: string, returnType: string) =
            Ast.Funs([ Ast.LongIdent parameter ], Ast.LongIdent returnType)

        static member Funs(parameters: string list, returnType: WidgetBuilder<Type>) =
            Ast.Funs(parameters |> List.map Ast.LongIdent, returnType)

        static member Funs(parameters: string, returnType: WidgetBuilder<Type>) =
            Ast.Funs([ Ast.LongIdent parameters ], returnType)

        static member Funs(parameters: string list, returnType: string) =
            Ast.Funs(parameters |> List.map Ast.LongIdent, Ast.LongIdent returnType)
