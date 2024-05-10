namespace Fabulous.AST.Tests.Types

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Types =
    (*
| Type      | .NET type | Description                                                                           | Example                    |
|-----------|-----------|---------------------------------------------------------------------------------------|----------------------------|
| bool      | Boolean   | Possible values are true and false.                                                  | true/false                 |
| byte      | Byte      | Values from 0 to 255.                                                                 | 1uy                        |
| sbyte     | SByte     | Values from -128 to 127.                                                              | 1y                         |
| int16     | Int16     | Values from -32768 to 32767.                                                          | 1s                         |
| uint16    | UInt16    | Values from 0 to 65535.                                                               | 1us                        |
| int       | Int32     | Values from -2,147,483,648 to 2,147,483,647.                                          | 1                          |
| uint      | UInt32    | Values from 0 to 4,294,967,295.                                                       | 1u                         |
| int64     | Int64     | Values from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.                  | 1L                         |
| uint64    | UInt64    | Values from 0 to 18,446,744,073,709,551,615.                                          | 1UL                        |
| nativeint | IntPtr    | A native pointer as a signed integer.                                                 | nativeint 1                |
| unativeint| UIntPtr   | A native pointer as an unsigned integer.                                              | unativeint 1               |
| decimal   | Decimal   | A floating point data type that has at least 28 significant digits.                    | 1.0m                       |
| float, double | Double | A 64-bit floating point type.                                                         | 1.0                        |
| float32, single | Single | A 32-bit floating point type.                                                         | 1.0f                       |
| char      | Char      | Unicode character values.                                                             | 'c'                        |
| string    | String    | Unicode text.                                                                         | "str"                      |
| unit      | NA        | Indicates the absence of an actual value. The type has only one formal value, which is denoted (). The unit value, (), is often used as a placeholder where a value is needed but no real value is available or makes sense. | () |
*)
    [<Fact>]
    let ``Value with return primitive types``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant "a"), ConstantExpr(Bool false)).returnType(Boolean())

                Value(ConstantPat(Constant "b"), ConstantExpr(Byte 0uy)).returnType(Byte())
                Value(ConstantPat(Constant "c"), ConstantExpr(SByte 1y)).returnType(SByte())
                Value(ConstantPat(Constant "d"), ConstantExpr(Int16 1s)).returnType(Int16())

                Value(ConstantPat(Constant "e"), ConstantExpr(UInt16 1us)).returnType(UInt16())

                Value(ConstantPat(Constant "f"), ConstantExpr(Int(1))).returnType(Int())
                Value(ConstantPat(Constant "g"), ConstantExpr(UInt32 1u)).returnType(UInt32())
                Value(ConstantPat(Constant "h"), ConstantExpr(Int64 1L)).returnType(Int64())

                Value(ConstantPat(Constant "i"), ConstantExpr(UInt64 1UL)).returnType(UInt64())

                Value(ConstantPat(Constant "j"), ConstantExpr(IntPtr(nativeint 1)))
                    .returnType(IntPtr())

                Value(ConstantPat(Constant "k"), ConstantExpr(UIntPtr(unativeint 1)))
                    .returnType(UIntPtr())

                Value(ConstantPat(Constant "l"), ConstantExpr(Decimal 1.0m))
                    .returnType(Decimal())

                Value(ConstantPat(Constant "m"), ConstantExpr(Constant "1.0"))
                    .returnType(Double())

                Value(ConstantPat(Constant "n"), ConstantExpr(Single 1.0f)).returnType(Single())

                Value(ConstantPat(Constant "o"), ConstantExpr(Char 'c')).returnType(Char())

                Value(ConstantPat(Constant "p"), ConstantExpr(String("str")))
                    .returnType(String())

                Value(ConstantPat(Constant "q"), ConstantExpr(Constant("()")))
                    .returnType(Unit())

                Value(ConstantPat(Constant "r"), ConstantExpr(Float 1.)).returnType(Float())

                Value(ConstantPat(Constant "s"), ConstantExpr(Float32 1.f))
                    .returnType(Float32())

                Value(ConstantPat(Constant "t"), ConstantExpr(Constant "obj")).returnType(Obj())
            }
        }
        |> produces
            """
let a: bool = false
let b: byte = 0uy
let c: sbyte = 1y
let d: int16 = 1s
let e: uint16 = 1us
let f: int = 1
let g: uint = 1u
let h: int64 = 1L
let i: uint64 = 1UL
let j: nativeint = nativeint 1
let k: unativeint = unativeint 1
let l: decimal = 1.0m
let m: double = 1.0
let n: single = 1.0f
let o: char = 'c'
let p: string = "str"
let q: unit = ()
let r: float = 1.0
let s: float32 = 1.0f
let t: obj = obj
"""

    [<Fact>]
    let ``Value with return non primitive types``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("a")), ConstantExpr(Bool(false)))
                    .returnType(StructTuple([ String(); String() ]))

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)))
                    .returnType(HashConstraint(String()))

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)))
                    .returnType(StaticConstant(String("A")))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(StaticConstantExpr("A", "B"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(AnonRecord([ "a", String(); "b", String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(StaticConstantNamed(String(), String()))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(Anon("A"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(Var("A"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(Array("string", 2))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(StructTuple([ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(AppPostfix(String(), String()))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(AppPostfix("string", "string"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(AppPrefix("Map", [ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(AppPrefix("A", "a", [ String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(Tuple([ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(MeasurePower("cm", Integer(2)))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)))
                    .returnType(Funs(String(), [ Int() ]))
            }
        }
        |> produces
            """
let a: struct (string , string) = false
let b: #string = false
let b: "A" = false
let c: A B = false
let c: {| a: string; b: string |} = false
let c: string=string = false
let c: A = false
let c: A = false
let c: string[,] = false
let c: struct (string , string) = false
let c: string string = false
let c: string string = false
let c: Map<string, string> = false
let c: A.a<string> = false
let c: string * string = false
let c: cm^2 = false
let c: int -> string = false
"""
