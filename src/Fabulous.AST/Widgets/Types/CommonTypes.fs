namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

// let Boolean = mkLongIdent "bool"
// let Byte = mkLongIdent "byte"
// let SByte = mkLongIdent "sbyte"
// let Int16 = mkLongIdent "int16"
// let Uint16 = mkLongIdent "uint16"
// let Int32 = mkLongIdent "int"
// let UInt32 = mkLongIdent "uint"
// let Int64 = mkLongIdent "int64"
// let Uint64 = mkLongIdent "uint64"
// let IntPtr = mkLongIdent "nativeint"
// let UIntPtr = mkLongIdent "unativeint"
// let Decimal = mkLongIdent "decimal"
// let Float = mkLongIdent "float"
// let Double = mkLongIdent "double"
// let Float32 = mkLongIdent "float32"
// let Single = mkLongIdent "single"
// let Char = mkLongIdent "char"
// let String = mkLongIdent "string"
// let Obj = mkLongIdent "obj"
// let Unit = mkLongIdent "unit"
[<AutoOpen>]
module CommonTypeBuilders =
    type Ast with
        static member Boolean() = Ast.TypeLongIdent("bool")

        static member Byte() = Ast.TypeLongIdent("byte")

        static member SByte() = Ast.TypeLongIdent("sbyte")

        static member Int16() = Ast.TypeLongIdent("int16")

        static member UInt16() = Ast.TypeLongIdent("uint16")

        static member Int32() = Ast.TypeLongIdent("int")

        static member UInt32() = Ast.TypeLongIdent("uint")

        static member Int64() = Ast.TypeLongIdent("int64")

        static member UInt64() = Ast.TypeLongIdent("uint64")

        static member IntPtr() = Ast.TypeLongIdent("nativeint")

        static member UIntPtr() = Ast.TypeLongIdent("unativeint")

        static member Decimal() = Ast.TypeLongIdent("decimal")

        static member Float() = Ast.TypeLongIdent("float")

        static member Double() = Ast.TypeLongIdent("double")

        static member Float32() = Ast.TypeLongIdent("float32")

        static member Single() = Ast.TypeLongIdent("single")

        static member Char() = Ast.TypeLongIdent("char")

        static member String() = Ast.TypeLongIdent("string")

        static member Obj() = Ast.TypeLongIdent("obj")

        static member Unit() = Ast.TypeLongIdent("unit")
