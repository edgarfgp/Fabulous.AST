(**
---
title: Expressions
category: widgets
index: 2
---
*)

(**
# Expressions
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
        IdentExpr("1")

        InterpolatedStringExpr(ConstantExpr("12"))

        InterpolatedRawStringExpr(ConstantExpr("12"))

        LazyExpr(Int(12))

        ListExpr([ String("a"); String("b"); String("c") ])

        ArrayExpr([ String("a"); String("b"); String("c") ])

        SeqExpr([ String("a"); String("b") ])

        InfixAppExpr(Int(1), "+", Int(2))

        StructTupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])

        TupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])

        Open("System")

        ChainExpr(
            [ ChainLinkExpr(String("string"))
              ChainLinkDot()
              ChainLinkExpr(OptVarExpr("Length")) ]
        )

        AppExpr("printfn", String("Hello, World!"))

        LambdaExpr(UnitPat(), Int(1))

        ParenLambdaExpr(ConstantPat("a"), ConstantExpr("a"))

        ParenLambdaExpr([ ConstantPat("a"); ConstantPat("b") ], ConstantExpr("a"))

        MatchExpr(
            Int(1),
            [ MatchClauseExpr(Int(1), String("a"))
              MatchClauseExpr(WildPat(), String("b")) ]
        )

        IfThenElseExpr(Bool(true), String("a"), String("b"))

        IfThenElifExpr(
            [ IfThenExpr(
                  InfixAppExpr(ConstantExpr(Constant "1"), "=", ConstantExpr(Int 12)),
                  ConstantExpr(ConstantUnit())
              )

              ElIfThenExpr(
                  InfixAppExpr(ConstantExpr(Constant("1")), "=", ConstantExpr(Int 11)),
                  ConstantExpr(ConstantUnit())
              ) ],
            ConstantExpr(ConstantUnit())
        )

        ForEachDoExpr("i", ListExpr([ Int(1); Int(2); Int(3) ]), AppExpr("printf", String("%i")))

        ForToExpr("i", ConstantExpr("1"), ConstantExpr("10"), ConstantExpr(ConstantUnit()))

        ForDownToExpr("i", ConstantExpr("1"), ConstantExpr("10"), ConstantExpr(ConstantUnit()))

        WhileExpr(ConstantExpr(Bool(true)), ConstantExpr(Int(0)))

        TryWithExpr(
            CompExprBodyExpr(
                [ LetOrUseExpr(Value("result", InfixAppExpr(Int(1), "/", Int(0))))
                  OtherExpr(AppExpr("printfn", [ String("%i"); Constant("result") ])) ]
            ),
            [ MatchClauseExpr("e", AppExpr("printfn", [ String("%s"); String("e.Message") ])) ]
        )

        TryFinallyExpr(Int(12), Int(12))

        TryWithSingleClauseExpr(Int(12), MatchClauseExpr(WildPat(), FailWithExpr(String("Not implemented"))))

        QuotedExpr(InfixAppExpr(Int(1), "+", Int(2)))

        MatchLambdaExpr([ MatchClauseExpr("a", Int(3)) ])

        NamedComputationExpr(ConstantExpr(Constant "task"), String("a"))

        NamedComputationExpr(Constant("task"), SingleExpr(SingleNode("return", String("a")).addSpace(true)))

        AnonRecordExpr([ RecordFieldExpr("A", Int(1)); RecordFieldExpr("B", Int(2)) ])

        AnonStructRecordExpr([ RecordFieldExpr("A", Int(1)); RecordFieldExpr("B", Int(2)) ])

        ObjExpr(LongIdent("System.Object"), ConstantExpr(ConstantUnit())) {
            Method("x.ToString()", [], ConstantExpr(String("F#")))
        }

        AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String("a")))

    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

1

$"{12}"

$"""{12}"""

lazy 12

[ "a"; "b"; "c" ]

[| "a"; "b"; "c" |]

seq {
    "a"
    "b"
}

1 + 2

struct (1, 2, 3)

1, 2, 3

open System

"string".Length

printfn "Hello, World!"

fun () -> 1

(fun a -> a)

(fun a b -> a)

match 1 with
| 1 -> "a"
| _ -> "b"

if true then "a" else "b"

if 1 = 12 then ()
elif 1 = 11 then ()
else ()

for i in [ 1; 2; 3 ] do
    printf "%i"

for i = 1 to 10 do
    ()

for i = 1 downto 10 do
    ()

while true do
    0

try
    let result = 1 / 2
    printfn "%i" result
with e ->
    printfn "%s" "e.Message"

try
    12
finally
    12

try
    12
with _ ->
    failwith "Not implemented"

<@ 1 + 2 @>

// function
// | a -> 3

// task { "a" }
// task { return "a" }

{| A = 1
   B = 2 |}

struct {| A = 1
          B = 2 |}

{ new System.Object() with
    member x.ToString() = "F#" }

printfn "a"
