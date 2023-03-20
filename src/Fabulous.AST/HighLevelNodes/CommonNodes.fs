namespace Fabulous.AST

open type Fabulous.AST.Ast

[<AutoOpen>]
module CommonNodes =
    type Fabulous.AST.Ast with
        static member inline ConstantTextExpr(text: string) =
            Expr(Constant(SingleText(text)))
            
        static member inline Let(name: string, value: string) =
            Binding(
                MultipleTexts() {
                    SingleText("let")
                },
                IdentList() {
                    Ident(SingleText(name))
                },
                SingleText("="),
                Ast.ConstantTextExpr(value)
            )
            
        static member inline Call(functionName: string, arg1: string, ?arg2: string) =
            let args = [|
                arg1
                match arg2 with None -> () | Some v -> v
            |]
            
            Expr(
                ExprApp(Ast.ConstantTextExpr(functionName)) {
                    for arg in args do
                        Ast.ConstantTextExpr(arg)
                }
            )