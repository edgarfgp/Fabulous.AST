namespace Fabulous.AST

[<AutoOpen>]
module CommonTypeBuilders =
    type Ast with
        static member Boolean() = Ast.LongIdent("bool")

        static member Byte() = Ast.LongIdent("byte")

        static member SByte() = Ast.LongIdent("sbyte")

        static member Int16() = Ast.LongIdent("int16")

        static member UInt16() = Ast.LongIdent("uint16")

        static member Int32() = Ast.LongIdent("int")

        static member UInt32() = Ast.LongIdent("uint")

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
