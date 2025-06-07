namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeLongIdent =

    let Value = Attributes.defineScalar<string seq> "Value"

    let WidgetKey =
        Widgets.register "LongIdentType" (fun widget ->
            let values =
                Widgets.getScalarValue widget Value
                |> Seq.intersperse "."
                |> Seq.map(fun value -> IdentifierOrDot.Ident(SingleTextNode.Create(value)))

            Type.LongIdent(IdentListNode(List.ofSeq values, Range.Zero)))

[<AutoOpen>]
module LongIdentTypeBuilders =
    type Ast with

        /// <summary>Creates a long identifier type with dot-separated parts.</summary>
        /// <param name="value">The seq of identifier parts.</param>
        static member LongIdent(value: string seq) =
            WidgetBuilder<Type>(TypeLongIdent.WidgetKey, TypeLongIdent.Value.WithValue(value))

        /// <summary>Creates a simple identifier type.</summary>
        /// <param name="value">The identifier name.</param>
        static member LongIdent(value: string) = Ast.LongIdent([ value ])

        /// <summary>Creates a boolean type (bool).</summary>
        static member Boolean() = Ast.LongIdent("bool")

        /// <summary>Creates a byte type.</summary>
        static member Byte() = Ast.LongIdent("byte")

        /// <summary>Creates a signed byte type (sbyte).</summary>
        static member SByte() = Ast.LongIdent("sbyte")

        /// <summary>Creates a 16-bit integer type (int16).</summary>
        static member Int16() = Ast.LongIdent("int16")

        /// <summary>Creates an unsigned 16-bit integer type (uint16).</summary>
        static member UInt16() = Ast.LongIdent("uint16")

        /// <summary>Creates a 32-bit integer type (int).</summary>
        static member Int() = Ast.LongIdent("int")

        /// <summary>Creates an unsigned 32-bit integer type (uint32).</summary>
        static member UInt32() = Ast.LongIdent("uint32")

        /// <summary>Creates a 64-bit integer type (int64).</summary>
        static member Int64() = Ast.LongIdent("int64")

        /// <summary>Creates an unsigned 64-bit integer type (uint64).</summary>
        static member UInt64() = Ast.LongIdent("uint64")

        /// <summary>Creates a native integer type (nativeint).</summary>
        static member IntPtr() = Ast.LongIdent("nativeint")

        /// <summary>Creates an unsigned native integer type (unativeint).</summary>
        static member UIntPtr() = Ast.LongIdent("unativeint")

        /// <summary>Creates a decimal type.</summary>
        static member Decimal() = Ast.LongIdent("decimal")

        /// <summary>Creates a floating-point type (float).</summary>
        static member Float() = Ast.LongIdent("float")

        /// <summary>Creates a double-precision floating-point type (double).</summary>
        static member Double() = Ast.LongIdent("double")

        /// <summary>Creates a 32-bit floating-point type (float32).</summary>
        static member Float32() = Ast.LongIdent("float32")

        /// <summary>Creates a single-precision floating-point type (single).</summary>
        static member Single() = Ast.LongIdent("single")

        /// <summary>Creates a character type (char).</summary>
        static member Char() = Ast.LongIdent("char")

        /// <summary>Creates a string type.</summary>
        static member String() = Ast.LongIdent("string")

        /// <summary>Creates an object type (obj).</summary>
        static member Obj() = Ast.LongIdent("obj")

        /// <summary>Creates a unit type.</summary>
        static member Unit() = Ast.LongIdent("unit")

        /// <summary>Creates an option type.</summary>
        static member Option() = Ast.LongIdent("option")

        /// <summary>Creates a list type.</summary>
        static member List() = Ast.LongIdent("list")

        /// <summary>Creates a sequence type (seq).</summary>
        static member Seq() = Ast.LongIdent("seq")

        /// <summary>Creates a null type.</summary>
        static member Null() = Ast.LongIdent("null")
