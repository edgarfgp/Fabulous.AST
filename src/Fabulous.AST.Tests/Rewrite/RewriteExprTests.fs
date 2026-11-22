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
        | Expr.Ident n when n.Text = oldName -> Expr.Ident(SingleTextNode(newName, Range.range0))
        | Expr.Constant(Constant.FromText n) when n.Text = oldName ->
            Expr.Constant(Constant.FromText(SingleTextNode(newName, Range.range0)))
        | _ -> e

    /// Compile a widget pipeline result to a formatted source string.
    let private render(w: WidgetBuilder<Oak>) : string = Gen.mkOak w |> Gen.run

    [<Fact>]
    let ``identity rewrite preserves source``() =
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("greet", "x + 1") } }

        let original = render widget
        let rewritten = widget |> Rewrite.expr id |> render
        Assert.Equal(original, rewritten)

    [<Fact>]
    let ``rename ident at top level``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() { Value("greet", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])) }
            }

        let source = widget |> Rewrite.expr(renameIdent "println" "printfn") |> render
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

        let source = widget |> Rewrite.expr(renameIdent "println" "printfn") |> render
        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``no match returns unchanged source``() =
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("greet", "x + 1") } }

        let original = render widget
        let rewritten = widget |> Rewrite.expr(renameIdent "println" "printfn") |> render
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

        let source = widget |> Rewrite.expr(renameIdent "println" "printfn") |> render
        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    [<Fact>]
    let ``exprInOak rewrites a raw Oak``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() { Value("greet", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])) }
            }

        let oak: Oak = Gen.mkOak widget
        let rewritten: Oak = oak |> Rewrite.exprInOak(renameIdent "println" "printfn")
        let source = Gen.run rewritten

        Assert.Contains("printfn", source)
        Assert.DoesNotContain("println", source)

    /// Turns a single-case union (`type T = T of a: .. * b: ..`) into the
    /// equivalent record (`type T = { a: ..; b: .. }`) by reusing the case's
    /// fields as the record fields.
    let private unionToRecord(td: TypeDefn) : TypeDefn =
        match td with
        | TypeDefn.Union n when n.UnionCases.Length = 1 ->
            TypeDefn.Record(
                TypeDefnRecordNode(
                    n.TypeName,
                    n.Accessibility,
                    SingleTextNode("{", Range.range0),
                    (n.UnionCases.Head.Fields |> List.map TypeDefnRecordFieldOrSpread.Field),
                    SingleTextNode("}", Range.range0),
                    None,
                    n.Members,
                    Range.range0
                )
            )
        | _ -> td

    [<Fact>]
    let ``typeDefn rewrites a single-case union into a record``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() { Union("Point") { UnionCase("Point", [ Field("X", Int()); Field("Y", Int()) ]) } }
            }

        let source = widget |> Rewrite.typeDefn unionToRecord |> render

        Assert.Contains("type Point = { X: int; Y: int }", source)
        Assert.DoesNotContain("| Point", source)

    [<Fact>]
    let ``expr rewrite keeps comments on rebuilt nodes``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() {
                    Value("x", AppExpr("f", "old"))
                        .triviaBefore(SingleLine("value comment"))
                        .triviaAfter(LineCommentAfterSourceCode("after"))

                    Value("y", ParenExpr(AppExpr("g", "old").triviaBefore(BlockComment("arg"))))

                    TypeDefn("C", UnitPat()) {
                        Member("this.M", AppExpr("h", "old")).triviaBefore(SingleLine("member comment"))
                    }
                }
            }

        let source = widget |> Rewrite.expr(renameIdent "old" "renamed") |> render

        Assert.Equal(
            """// value comment
let x = f renamed // after
let y = ( (*arg*) g renamed)

type C() =
    // member comment
    member this.M = h renamed
""",
            source
        )

    [<Fact>]
    let ``typeDefn rewrite keeps comments on a rebuilt nested module``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() {
                    Module("Shapes") { Union("Point") { UnionCase("Point", [ Field("X", Int()); Field("Y", Int()) ]) } }
                    |> _.triviaBefore(SingleLine("module comment"))
                }
            }

        let source = widget |> Rewrite.typeDefn unionToRecord |> render

        Assert.Equal(
            """// module comment
module Shapes =
    type Point = { X: int; Y: int }
""",
            source
        )
