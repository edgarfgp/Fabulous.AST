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
                Value(ConstantPat(Constant "a"), ConstantExpr(Bool false), Boolean())

                Value(ConstantPat(Constant "b"), ConstantExpr(Byte 0uy), Byte())
                Value(ConstantPat(Constant "c"), ConstantExpr(SByte 1y), SByte())
                Value(ConstantPat(Constant "d"), ConstantExpr(Int16 1s), Int16())

                Value(ConstantPat(Constant "e"), ConstantExpr(UInt16 1us), UInt16())

                Value(ConstantPat(Constant "f"), ConstantExpr(Int(1)), Int())

                Value(ConstantPat(Constant "g"), ConstantExpr(UInt32 1u), UInt32())

                Value(ConstantPat(Constant "h"), ConstantExpr(Int64 1L), Int64())

                Value(ConstantPat(Constant "i"), ConstantExpr(UInt64 1UL), UInt64())

                Value(ConstantPat(Constant "j"), ConstantExpr(IntPtr(nativeint 1)), IntPtr())

                Value(ConstantPat(Constant "k"), ConstantExpr(UIntPtr(unativeint 1)), UIntPtr())

                Value(ConstantPat(Constant "l"), ConstantExpr(Decimal 1.0m), Decimal())

                Value(ConstantPat(Constant "m"), ConstantExpr(Constant "1.0"), Double())

                Value(ConstantPat(Constant "n"), ConstantExpr(Single 1.0f), Single())

                Value(ConstantPat(Constant "o"), ConstantExpr(Char 'c'), Char())

                Value(ConstantPat(Constant "p"), ConstantExpr(String("str")), String())

                Value(ConstantPat(Constant "q"), ConstantExpr(Constant("()")), Unit())

                Value(ConstantPat(Constant "r"), ConstantExpr(Float 1.), Float())

                Value(ConstantPat(Constant "s"), ConstantExpr(Float32 1.f), Float32())

                Value(ConstantPat(Constant "t"), ConstantExpr(Constant "obj"), Obj())
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
let g: uint32 = 1u
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
                Value(
                    ConstantPat(Constant("a")),
                    ConstantExpr(Bool(false)),
                    returnType = StructTuple([ String(); String() ])
                )

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)), HashConstraint(String()))

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)), StaticConstant(String("A")))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), StaticConstantExpr("A", "B"))

                Value(
                    ConstantPat(Constant("c")),
                    ConstantExpr(Bool(false)),
                    AnonRecord([ "a", String(); "b", String() ])
                )

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), StaticConstantNamed(String(), String()))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), Anon("A"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), Var("A"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), Array("string", 2))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), StructTuple([ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPostfix(String(), String()))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPostfix("string", "string"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPrefix("Map", [ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPrefix("A", "a", [ String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), Tuple([ String(); String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), MeasurePowerType("cm", Integer(2)))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), Funs([ Int() ], String()))
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

    [<Fact>]
    let ``Value with AppPostfix widget``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("a")), ConstantExpr(Bool(false)), AppPostfix(String(), Option()))

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)), AppPostfix("string", "option"))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPostfix(String(), "option"))

                Value(ConstantPat(Constant("d")), ConstantExpr(Bool(false)), AppPostfix("string", Option()))

                Value(ConstantPat(Constant("e")), ConstantExpr(Bool(false)), OptionPostfix(String()))

                Value(ConstantPat(Constant("f")), ConstantExpr(Bool(false)), OptionPostfix("string"))

                Value(ConstantPat(Constant("g")), ConstantExpr(Bool(false)), VOptionPostfix(String()))

                Value(ConstantPat(Constant("h")), ConstantExpr(Bool(false)), VOptionPostfix("string"))

                Value(ConstantPat(Constant("i")), ConstantExpr(Bool(false)), ListPostfix(String()))

                Value(ConstantPat(Constant("j")), ConstantExpr(Bool(false)), ListPostfix("string"))

                Value(ConstantPat(Constant("k")), ConstantExpr(Bool(false)), SeqPostfix(String()))

                Value(ConstantPat(Constant("l")), ConstantExpr(Bool(false)), SeqPostfix("string"))

                Value(ConstantPat(Constant("m")), ConstantExpr(Bool(false)), Array(String()))

                Value(ConstantPat(Constant("n")), ConstantExpr(Bool(false)), Array(Array(String())))

            }
        }
        |> produces
            """
let a: string option = false
let b: string option = false
let c: string option = false
let d: string option = false
let e: string option = false
let f: string option = false
let g: string voption = false
let h: string voption = false
let i: string list = false
let j: string list = false
let k: string seq = false
let l: string seq = false
let m: string[] = false
let n: string[][] = false
"""

    [<Fact>]
    let ``Value with AppPrefix widget``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("a")), ConstantExpr(Bool(false)), AppPrefix(Option(), [ String() ]))

                Value(ConstantPat(Constant("b")), ConstantExpr(Bool(false)), AppPrefix("option", [ String() ]))

                Value(ConstantPat(Constant("c")), ConstantExpr(Bool(false)), AppPrefix(List(), [ String() ]))

                Value(ConstantPat(Constant("d")), ConstantExpr(Bool(false)), AppPrefix("list", [ String() ]))

                Value(ConstantPat(Constant("d")), ConstantExpr(Bool(false)), AppPrefix("list", [ "string" ]))

                Value(ConstantPat(Constant("e")), ConstantExpr(Bool(false)), AppPrefix(Seq(), [ String(); String() ]))

                Value(ConstantPat(Constant("f")), ConstantExpr(Bool(false)), AppPrefix(Option(), [ String() ]))

                Value(ConstantPat(Constant("g")), ConstantExpr(Bool(false)), AppPrefix("option", String()))

                Value(ConstantPat(Constant("h")), ConstantExpr(Bool(false)), AppPrefix(List(), String()))

                Value(ConstantPat(Constant("i")), ConstantExpr(Bool(false)), OptionPrefix(String()))

                Value(ConstantPat(Constant("j")), ConstantExpr(Bool(false)), OptionPrefix("string"))

                Value(ConstantPat(Constant("k")), ConstantExpr(Bool(false)), VOptionPrefix(String()))

                Value(ConstantPat(Constant("l")), ConstantExpr(Bool(false)), VOptionPrefix("string"))

                Value(ConstantPat(Constant("m")), ConstantExpr(Bool(false)), ListPrefix(String()))

                Value(ConstantPat(Constant("n")), ConstantExpr(Bool(false)), ListPrefix("string"))

                Value(ConstantPat(Constant("o")), ConstantExpr(Bool(false)), SeqPrefix(String()))

                Value(ConstantPat(Constant("p")), ConstantExpr(Bool(false)), SeqPrefix("string"))

                Value(ConstantPat(Constant("q")), ConstantExpr(Bool(false)), ResultPrefix(String(), String()))

                Value(ConstantPat(Constant("r")), ConstantExpr(Bool(false)), ResultPrefix("string", "string"))

                Value(ConstantPat(Constant("s")), ConstantExpr(Bool(false)), ResultPrefix("string", String()))

                Value(ConstantPat(Constant("t")), ConstantExpr(Bool(false)), ResultPrefix(String(), "string"))

                Value(ConstantPat(Constant("r")), ConstantExpr(Bool(false)), ArrayPrefix(String()))

                Value(ConstantPat(Constant("s")), ConstantExpr(Bool(false)), ArrayPrefix("string"))

                Value(ConstantPat(Constant("t")), ConstantExpr(Bool(false)), ArrayPrefix(ArrayPrefix(String())))

                Value(ConstantPat(Constant("u")), ConstantExpr(Bool(false)), AsyncPrefix(String()))

                Value(ConstantPat(Constant("v")), ConstantExpr(Bool(false)), TaskPrefix(String()))
            }
        }
        |> produces
            """
let a: option<string> = false
let b: option<string> = false
let c: list<string> = false
let d: list<string> = false
let d: list<string> = false
let e: seq<string, string> = false
let f: option<string> = false
let g: option<string> = false
let h: list<string> = false
let i: option<string> = false
let j: option<string> = false
let k: voption<string> = false
let l: voption<string> = false
let m: list<string> = false
let n: list<string> = false
let o: seq<string> = false
let p: seq<string> = false
let q: Result<string, string> = false
let r: Result<string, string> = false
let s: Result<string, string> = false
let t: Result<string, string> = false
let r: array<string> = false
let s: array<string> = false
let t: array<array<string>> = false
let u: Async<string> = false
let v: Task<string> = false
"""

    [<Fact>]
    let ``Value with Type Constraint widgets``() =
        Oak() {
            AnonymousModule() {
                // Base Type Constraint
                ClassEnd("Class1") { () }
                |> _.typeParams(PostfixList("'T when 'T :> System.Exception"))

                ClassEnd("Class11") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), SubtypeOf("'T", LongIdent("System.Exception"))))

                ClassEnd("Class2") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), SubtypeOf("'T", LongIdent("System.IComparable"))))

                ClassEnd("Class3") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "null")))

                ClassEnd("Class5") { () }
                |> _.typeParams(
                    PostfixList(
                        TyparDecl("'T"),
                        Supports("'T", SigMember(Val("member", "Method1", LongIdent("'T -> int"))))
                    )
                )

                ClassEnd("Class6") { () }
                |> _.typeParams(
                    PostfixList(TyparDecl("'T"), Supports("'T", SigMember(Val("member", "Property1", Int()))))
                )

                TypeDefn("Class7", UnitPat()) { MemberVal("Field", NewExpr("'T", ConstantExpr(ConstantUnit()))) }
                |> _.typeParams(
                    PostfixList(TyparDecl("'T"), Supports("'T", SigMember(Val("new", "", Funs([ "unit" ], "'T")))))
                )

                ClassEnd("Class8") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "not struct")))

                // Enumeration constraint with underlying value specified
                ClassEnd("Class9") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), EnumOrDelegate("'T", "enum", UInt32())))

                // 'T must implement IComparable, or be an array type with comparable
                // elements, or be System.IntPtr or System.UIntPtr. Also, 'T must not have
                // the NoComparison attribute.
                ClassEnd("Class10") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "comparison")))

                // 'T must support equality. This is true for any type that does not
                // have the NoEquality attribute.
                ClassEnd("Class11") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "equality")))

                ClassEnd("Class12") { () }
                |> _.typeParams(
                    PostfixList(TyparDecl("'T"), EnumOrDelegate("'T", "delegate", [ "obj * System.EventArgs"; "unit" ]))
                )

                ClassEnd("Class122") { () }
                |> _.typeParams(
                    PostfixList(
                        TyparDecl("'T"),
                        EnumOrDelegate("'T", "delegate", [ Tuple([ "obj"; "System.EventArgs" ]); Unit() ])
                    )
                )

                ClassEnd("Class13") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "unmanaged")))

                // Member constraints with two type parameters
                // Most often used with static type parameters in inline functions
                Function(
                    "add",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(
                                  NamedPat("value1"),
                                  WithGlobal(
                                      "^T",
                                      Supports(
                                          "^T",
                                          SigMember(Val([ "static"; "member" ], "(+)", LongIdent("^T * ^T -> ^T")))
                                      )
                                  )
                              )

                              ParameterPat(NamedPat("value2"), "^T") ]
                        )
                    ),
                    InfixAppExpr("value1", "+", "value2")
                )
                    .toInlined()

                // ^T and ^U must support operator +
                Function(
                    "heterogenousAdd",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(
                                  NamedPat("value1"),
                                  WithGlobal(
                                      "^T",
                                      Supports(
                                          Paren(Or("^T", "^U")),
                                          SigMember(Val([ "static"; "member" ], "(+)", LongIdent("^T * ^U -> ^T")))
                                      )
                                  )
                              )

                              ParameterPat(NamedPat("value2"), "^U") ]
                        )
                    ),
                    InfixAppExpr("value1", "+", "value2")
                )
                    .toInlined()

                // If there are multiple constraints, use the and keyword to separate them.
                ClassEnd("Class14") { () }
                |> _.typeParams(
                    PostfixList(
                        [ TyparDecl("'T"); TyparDecl("'U") ],
                        [ ConstraintSingle("'T", "equality"); ConstraintSingle("'U", "equality") ]
                    )
                )
            }
        }
        |> produces
            """
type Class1<'T when 'T :> System.Exception> = class end
type Class11<'T when 'T :> System.Exception> = class end
type Class2<'T when 'T :> System.IComparable> = class end
type Class3<'T when 'T: null> = class end
type Class5<'T when 'T: (member Method1: 'T -> int)> = class end
type Class6<'T when 'T: (member Property1: int)> = class end

type Class7<'T when 'T: (new : unit -> 'T)>() =
    member val Field = new 'T ()

type Class8<'T when 'T: not struct> = class end
type Class9<'T when 'T: enum<uint32>> = class end
type Class10<'T when 'T: comparison> = class end
type Class11<'T when 'T: equality> = class end
type Class12<'T when 'T: delegate<obj * System.EventArgs, unit>> = class end
type Class122<'T when 'T: delegate<obj * System.EventArgs, unit>> = class end
type Class13<'T when 'T: unmanaged> = class end
let inline add (value1: ^T when ^T: (static member (+): ^T * ^T -> ^T), value2: ^T) = value1 + value2

let inline heterogenousAdd (value1: ^T when (^T or ^U): (static member (+): ^T * ^U -> ^T), value2: ^U) =
    value1 + value2

type Class14<'T, 'U when 'T: equality and 'U: equality> = class end
"""

    [<Fact>]
    let ``Intersection type widgets``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "test",
                    ParenPat(
                        ParameterPat(
                            NamedPat("env"),
                            Intersection(
                                [ LongIdent("'t")
                                  HashConstraint("System.Numerics.INumber<'t>")
                                  HashConstraint("IEquatable<'t>") ]
                            )
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let test (env: 't & #System.Numerics.INumber<'t> & #IEquatable<'t>) = ()
"""

    [<Fact>]
    let ``TypeHashConstraint type widgets``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "test",
                    ParenPat(
                        ParameterPat(
                            ParameterPat(NamedPat("env"), HashConstraint("ILogger1")),
                            HashConstraint("ILogger2")
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let test (env: #ILogger1: #ILogger2) = ()
"""
