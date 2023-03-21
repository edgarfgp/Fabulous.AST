namespace Fabulous.AST.Tests

open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module NodesTests =
    [<Test>]
    let CanCompileBasicTree () =
        Oak() {
            ModuleOrNamespace() {
                Binding(
                    MultipleTexts() {
                        SingleText("let")
                    },
                    IdentList() {
                        IdentifierOrDot_Ident(SingleText("hello"))
                    },
                    SingleText("="),
                    ConstantTextExpr("\"World\"")
                )
            }
        }
        |> produces "let hello = \"World\""
        
    [<Test>]
    let CanCompileBasicTree2 () =
        Oak() {
            ModuleOrNamespace() {
                Let("x", "123")
            }
        }
        |> produces "let x = 123"
        
    [<Test>]
    let ``Can produce if-then`` () =
        Oak() {
            ModuleOrNamespace() {
                Expr(
                    ExprIfThen(
                        IfKeyword(SingleText("if")),
                        Expr(
                            ExprInfixApp(
                                Expr_Ident(SingleText("x")),
                                SingleText("="),
                                Expr_Ident(SingleText("1"))
                            )
                        ),
                        SingleText("then"),
                        Expr(Constant(Unit()))
                    )
                )
            }
        }
        |> produces """

if x = 1 then
    ()

"""

    [<Test>]
    let z () =
        let source =
            """

if x = 1 then
    ()

"""
        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
        Assert.NotNull(rootNode)