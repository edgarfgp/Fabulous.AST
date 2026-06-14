namespace AstEditor

open System

/// A Fabulous.AST `Rewrite`-powered code action: rewrite the DSL so its pipeline runs a
/// constant-folding `Rewrite.expr` pass. It adds the `foldConstants` helper (and the opens it
/// needs) and threads `|> Rewrite.expr foldConstants` in before `|> Gen.mkOak` — after which
/// the live output shows trivial arithmetic (x*1, x+0, literal folds) simplified.
module RewriteAction =

    let private helper =
        String.concat
            "\n"
            [ "let intExpr (value: int) : Expr ="
              "    Expr.Constant(Constant.FromText(SingleTextNode(string value, Range.Zero)))"
              ""
              "let (|IntLit|_|) (e: Expr) : int option ="
              "    match e with"
              "    | Expr.Constant(Constant.FromText n) ->"
              "        match System.Int32.TryParse n.Text with"
              "        | true, v -> Some v"
              "        | _ -> None"
              "    | _ -> None"
              ""
              "// Constant-fold trivial arithmetic: x*1, x+0, and literal folds."
              "let foldConstants (e: Expr) : Expr ="
              "    match e with"
              "    | Expr.InfixApp n ->"
              "        match n.Operator.Text, n.LeftHandSide, n.RightHandSide with"
              "        | \"+\", lhs, IntLit 0"
              "        | \"+\", IntLit 0, lhs"
              "        | \"*\", lhs, IntLit 1"
              "        | \"*\", IntLit 1, lhs -> lhs"
              "        | \"+\", IntLit a, IntLit b -> intExpr (a + b)"
              "        | \"*\", IntLit a, IntLit b -> intExpr (a * b)"
              "        | _ -> e"
              "    | _ -> e"
              "" ]

    /// 1-based line of the first `|> Gen.mkOak` outside a line comment, if any — where the
    /// lightbulb anchors and proof there's a real pipeline to thread into.
    let anchorLine(source: string) : int option =
        source.Replace("\r\n", "\n").Split('\n')
        |> Array.tryFindIndex(fun l ->
            not(l.TrimStart().StartsWith("//", StringComparison.Ordinal))
            && l.Contains "|> Gen.mkOak")
        |> Option.map(fun i -> i + 1)

    /// Applicable when there's a pipeline to thread into and no Rewrite pass already.
    let canApply(source: string) =
        (anchorLine source).IsSome && not(source.Contains "Rewrite.expr")

    /// The rewritten source, or None if the action doesn't apply.
    let addConstantFolding(source: string) : string option =
        if not(canApply source) then
            None
        else
            let s = source.Replace("\r\n", "\n")

            // foldConstants needs the Fantomas Oak/Text types; make sure they're opened.
            let ensureOpen (op: string) (text: string) =
                if text.Contains op then
                    text
                elif text.Contains "open Fabulous.AST\n" then
                    text.Replace("open Fabulous.AST\n", "open Fabulous.AST\n" + op + "\n")
                else
                    op + "\n" + text

            let withOpens =
                s
                |> ensureOpen "open Fantomas.Core.SyntaxOak"
                |> ensureOpen "open Fantomas.FCS.Text"

            let withRewrite =
                withOpens.Replace("|> Gen.mkOak", "|> Rewrite.expr foldConstants\n|> Gen.mkOak")

            // Drop the helper in just before the Oak() expression (after the opens).
            let marker = "\nOak("
            let idx = withRewrite.IndexOf(marker, StringComparison.Ordinal)

            if idx >= 0 then
                Some(withRewrite.Insert(idx + 1, helper + "\n"))
            else
                // No `\nOak(` anchor (e.g. the user wrote `Oak () {`). Insert after the last
                // `open …` line instead — never above it, or the helper's Expr/SingleTextNode
                // references would precede the opens they need.
                let lines = withRewrite.Split('\n')

                match
                    lines
                    |> Array.tryFindIndexBack(fun l -> l.TrimStart().StartsWith("open ", StringComparison.Ordinal))
                with
                | Some i ->
                    let before = lines.[..i] |> String.concat "\n"
                    let after = lines.[i + 1 ..] |> String.concat "\n"
                    Some(before + "\n" + helper + "\n" + after)
                | None -> Some(helper + "\n" + withRewrite)
