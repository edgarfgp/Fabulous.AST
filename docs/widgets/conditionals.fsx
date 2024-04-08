(**
---
title: Conditionals
category: widgets
index: 5
---
*)

(**
# Conditionals

The if...then...else expression runs different branches of code and also evaluates to a different value depending on the Boolean expression given.
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| IfThenExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) | Creates a new if...then expression. |
| IfThenExpr(ifExpr: StringVariant, thenExpr: StringVariant) | Creates a new if...then expression with string variants. |
| ElIfThenExpr(elIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) | Creates a new elif...then expression. |
| ElIfThenExpr(elIfExpr: StringVariant, thenExpr: StringVariant) | Creates a new elif...then expression with string variants. |
| ElseIfThenExpr(elseIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) | Creates a new else if...then expression. |
| ElseIfThenExpr(elseIfExpr: StringVariant, thenExpr: StringVariant) | Creates a new else if...then expression with string variants. |
| IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: WidgetBuilder<Expr>) | Creates a new if...then...elif expression. |
| IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: StringVariant) | Creates a new if...then...elif expression with string variants. |
| IfThenElifExpr(branches: WidgetBuilder<Expr> list) | Creates a new if...then...elif expression. |
| IfThenElseExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>) | Creates a new if...then...else expression. |
| IfThenElseExpr(ifExpr: StringVariant, thenExpr: StringVariant, elseExpr: StringVariant) | Creates a new if...then...else expression with string variants. |

*)

(**

## Usage

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
        Value("x", ConstantExpr(Unquoted "12"))
        IfThenExpr(InfixAppExpr(Unquoted "x", "=", Unquoted "12"), ConstantExpr(ConstantUnit()))
        ElIfThenExpr(InfixAppExpr(Unquoted "x", "=", Unquoted "12"), ConstantExpr(ConstantUnit()))

        IfThenElseExpr(
            InfixAppExpr(Unquoted "x", "=", Unquoted "12"),
            ConstantExpr(ConstantUnit()),
            ConstantExpr(ConstantUnit())
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
let x = 12

if x = 12 then
    ()

elif x = 12 then
    ()

if x = 12 then () else ()
