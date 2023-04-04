namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList


module UnionCase =

    let Name = Attributes.defineWidget "SingleTextNode"

    let Fields = Attributes.defineScalar<FieldType list> "Fields"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let fields = Helpers.tryGetScalarValue widget Fields |> ValueOption.defaultValue([])

            let fields =
                fields
                |> List.map(fun field ->
                    match field with
                    | Type t ->
                        FieldNode(
                            None,
                            None,
                            None,
                            false,
                            None,
                            None,
                            Type.Var(SingleTextNode(t, Range.Zero)),
                            Range.Zero
                        )
                    | NameAndType(name, fieldType) ->
                        FieldNode(
                            None,
                            None,
                            None,
                            false,
                            None,
                            Some(SingleTextNode(name, Range.Zero)),
                            Type.Var(SingleTextNode(fieldType, Range.Zero)),
                            Range.Zero
                        ))

            UnionCaseNode(None, None, None, name, fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Fabulous.AST.Ast with

        static member inline UnionCase(name: WidgetBuilder<#SingleTextNode>, ?fields: FieldType list) =
            match fields with
            | Some fields ->
                WidgetBuilder<UnionCaseNode>(
                    UnionCase.WidgetKey,
                    AttributesBundle(
                        StackList.one(UnionCase.Fields.WithValue(fields)),
                        ValueSome [| UnionCase.Name.WithValue(name.Compile()) |],
                        ValueNone
                    )
                )
            | None ->
                WidgetBuilder<UnionCaseNode>(
                    UnionCase.WidgetKey,
                    AttributesBundle(
                        StackList.empty(),
                        ValueSome [| UnionCase.Name.WithValue(name.Compile()) |],
                        ValueNone
                    )
                )

        static member inline UnionCase(node: SingleTextNode, ?fields: FieldType list) =
            match fields with
            | Some fields -> Ast.UnionCase(Ast.EscapeHatch(node), fields)
            | None -> Ast.UnionCase(Ast.EscapeHatch(node))

        static member inline UnionCase(name: string, ?fields: FieldType list) =
            match fields with
            | Some fields -> Ast.UnionCase(SingleTextNode(name, Range.Zero), fields)
            | None -> Ast.UnionCase(SingleTextNode(name, Range.Zero))

[<Extension>]
type UnionCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>,
            x: WidgetBuilder<UnionCaseNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
