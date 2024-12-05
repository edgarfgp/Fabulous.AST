(**
---
title: Types
category: widgets
index: 9
---
*)

(**
# Types
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Value(ConstantPat(Constant "a"), Bool(false)).returnType(Boolean())

        Value(ConstantPat(Constant "b"), Byte(0uy)).returnType(Byte())
        Value(ConstantPat(Constant "c"), SByte(1y)).returnType(SByte())
        Value(ConstantPat(Constant "d"), Int16(1s)).returnType(Int16())

        Value(ConstantPat(Constant "e"), UInt16(1us)).returnType(UInt16())

        Value(ConstantPat(Constant "f"), Int(1)).returnType(Int())
        Value(ConstantPat(Constant "g"), UInt32(1u)).returnType(UInt32())
        Value(ConstantPat(Constant "h"), Int64(1L)).returnType(Int64())

        Value(ConstantPat(Constant "i"), UInt64(1UL)).returnType(UInt64())

        Value(ConstantPat(Constant "j"), IntPtr(nativeint 1)).returnType(IntPtr())

        Value(ConstantPat(Constant "k"), UIntPtr(unativeint 1)).returnType(UIntPtr())

        Value(ConstantPat(Constant "l"), Decimal(1.0m)).returnType(Decimal())

        Value(ConstantPat(Constant "m"), Constant("1.0")).returnType(Double())

        Value(ConstantPat(Constant "n"), Single(1.0f)).returnType(Single())

        Value(ConstantPat(Constant "o"), Char('c')).returnType(Char())

        Value(ConstantPat(Constant "p"), String("str")).returnType(String())

        Value(ConstantPat(Constant "q"), Constant("()")).returnType(Unit())

        Value(ConstantPat(Constant "r"), Float(1.)).returnType(Float())

        Value(ConstantPat(Constant "s"), Float32(1.f)).returnType(Float32())

        Value(ConstantPat(Constant "t"), Constant("System.Object()")).returnType(Obj())

        Value("u", ArrayExpr([ Int(1); Int(2) ])).returnType(Array(Int()))

        Value("v", ListExpr([ Int(1); Int(2) ])).returnType(ListPostfix(Int()))

        Value("w", SeqExpr([ Int(1); Int(2); Int(3) ])).returnType(SeqPostfix(Int()))

        Value("x", TupleExpr([ Int(1); Int(2) ])).returnType(Tuple([ Int(); Int() ]))

        Value("y", AppExpr("Some", Int(1))).returnType(OptionPostfix(Int()))

        Value("z", StructTupleExpr([ Int(1); String("") ]))
            .returnType(StructTuple([ Int(); String() ]))

        Value("funs", LambdaExpr(UnitPat(), ConstantUnit()))
            .returnType(Funs(Unit(), Unit()))

        Value("anonRecord", AnonRecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 1)) ]))
            .returnType(AnonRecord([ "A", Int() ]))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
