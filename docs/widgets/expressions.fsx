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

        ListExpr([ String("a"); String("b"); String("c") ])

        ArrayExpr([ String("a"); String("b"); String("c") ])

        InfixAppExpr(Int(1), "+", Int(2))

        Open("System")

        ChainExpr(
            [ ChainLinkExpr(String("string"))
              ChainLinkDot()
              ChainLinkExpr(OptVarExpr("Length")) ]
        )

        AppExpr("printfn", String("Hello, World!"))

        LambdaExpr(UnitPat(), Int(1))

        MatchExpr(
            Int(1),
            [ MatchClauseExpr(Int(1), String("a"))
              MatchClauseExpr(WildPat(), String("b")) ]
        )

        IfThenElseExpr(Bool(true), String("a"), String("b"))

        ForEachDoExpr("i", ListExpr([ Int(1); Int(2); Int(3) ]), AppExpr("printf", String("%i")))
        ForEachArrowExpr("i", ListExpr([ Int(1); Int(2); Int(3) ]), AppExpr("printf", String("%i")))

        TryWithExpr(
            CompExprBodyExpr(
                [ LetOrUseExpr(Value("result", InfixAppExpr(Int(1), "/", Int(0))))
                  OtherExpr(AppExpr("printfn", [ String("%i"); String("result") ])) ]
            ),
            [ MatchClauseExpr("e", AppExpr("printfn", [ String("%s"); String("e.Message") ])) ]
        )
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

// https://fsharpforfunandprofit.com/posts/understanding-fsharp-expressions/
