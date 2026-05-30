namespace Fabulous.AST.Tests.Rewrite

open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit

open type Ast

module RewriteExprTests =

    /// Renames any leaf expression (Ident or Constant.FromText) matching
    /// <paramref name="oldName"/> to <paramref name="newName"/>. The widget DSL
    /// emits constants for raw identifier text (`ConstantExpr(Constant "x")`
    /// → <c>Expr.Constant</c>), so a real-world rewrite must cover both shapes.
    let renameIdent (oldName: string) (newName: string) (e: Expr) : Expr =
        match e with
        | Expr.Ident n when n.Text = oldName -> Expr.Ident(SingleTextNode(newName, Range.Zero))
        | Expr.Constant(Constant.FromText n) when n.Text = oldName ->
            Expr.Constant(Constant.FromText(SingleTextNode(newName, Range.Zero)))
        | _ -> e

    /// Compile a widget pipeline result to a formatted source string.
    let private render(w: WidgetBuilder<Oak>) : string = Gen.mkOak w |> Gen.run

    [<Fact>]
    let ``identity rewrite preserves source``() =
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("greet", "x + 1") } }

        let original = render widget
        let rewritten = Rewrite.expr(id, widget) |> render
        Assert.Equal(original, rewritten)

    [<Fact>]
    let ``rename ident at top level``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() { Value("greet", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])) }
            }

        let source = Rewrite.expr(renameIdent "println" "printfn", widget) |> render
        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``rename ident inside nested application``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() {
                    Value(
                        "go",
                        AppExpr(
                            ConstantExpr(Constant "List.iter"),
                            [ ConstantExpr(Constant "println"); ConstantExpr(Constant "xs") ]
                        )
                    )
                }
            }

        let source = Rewrite.expr(renameIdent "println" "printfn", widget) |> render
        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``no match returns unchanged source``() =
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("greet", "x + 1") } }

        let original = render widget
        let rewritten = Rewrite.expr(renameIdent "println" "printfn", widget) |> render
        Assert.Equal(original, rewritten)

    [<Fact>]
    let ``rewrite reaches expr inside type-member body``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() {
                    TypeDefn("Greeter") {
                        Member("this.Hello", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ]))
                    }
                }
            }

        let source = Rewrite.expr(renameIdent "println" "printfn", widget) |> render
        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``raw Oak overload returns Oak``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() { Value("greet", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])) }
            }

        let oak: Oak = Gen.mkOak widget
        let rewritten: Oak = Rewrite.exprInOak(renameIdent "println" "printfn", oak)
        let source = Gen.run rewritten

        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``Expr-only overload returns Expr``() =
        let exprWidget: WidgetBuilder<Expr> =
            AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])

        let expr: Expr = Gen.mkOak exprWidget
        let rewritten: Expr = Rewrite.expr(renameIdent "println" "printfn", expr)

        match rewritten with
        | Expr.App n ->
            match n.FunctionExpr with
            | Expr.Constant(Constant.FromText t) -> Assert.Equal("printfn", t.Text)
            | _ -> Assert.Fail "expected function-expr to be Constant.FromText"
        | _ -> Assert.Fail "expected App node"
