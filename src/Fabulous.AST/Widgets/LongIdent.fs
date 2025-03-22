namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeLongIdent =

    let Value = Attributes.defineScalar<string list> "Value"

    let WidgetKey =
        Widgets.register "LongIdentType" (fun widget ->
            let values =
                Widgets.getScalarValue widget Value
                |> List.intersperse "."
                |> List.map(fun value -> IdentifierOrDot.Ident(SingleTextNode.Create(value)))

            Type.LongIdent(IdentListNode(values, Range.Zero)))

[<AutoOpen>]
module LongIdentTypeBuilders =
    type Ast with

        static member LongIdent(value: string list) =
            WidgetBuilder<Type>(TypeLongIdent.WidgetKey, TypeLongIdent.Value.WithValue(value))

        static member LongIdent(value: string) = Ast.LongIdent([ value ])

        static member Boolean() = Ast.LongIdent("bool")

        static member Byte() = Ast.LongIdent("byte")

        static member SByte() = Ast.LongIdent("sbyte")

        static member Int16() = Ast.LongIdent("int16")

        static member UInt16() = Ast.LongIdent("uint16")

        static member Int() = Ast.LongIdent("int")

        static member UInt32() = Ast.LongIdent("uint32")

        static member Int64() = Ast.LongIdent("int64")

        static member UInt64() = Ast.LongIdent("uint64")

        static member IntPtr() = Ast.LongIdent("nativeint")

        static member UIntPtr() = Ast.LongIdent("unativeint")

        static member Decimal() = Ast.LongIdent("decimal")

        static member Float() = Ast.LongIdent("float")

        static member Double() = Ast.LongIdent("double")

        static member Float32() = Ast.LongIdent("float32")

        static member Single() = Ast.LongIdent("single")

        static member Char() = Ast.LongIdent("char")

        static member String() = Ast.LongIdent("string")

        static member Obj() = Ast.LongIdent("obj")

        static member Unit() = Ast.LongIdent("unit")

        static member Option() = Ast.LongIdent("option")

        static member List() = Ast.LongIdent("list")

        static member Seq() = Ast.LongIdent("seq")

        static member Null() = Ast.LongIdent("null")
