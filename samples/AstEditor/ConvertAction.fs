namespace AstEditor

open System
open System.Text.RegularExpressions

/// A DSL-restructuring code action: convert a `Record("X") { Field… }` block into a
/// `Union("X") { UnionCase… }` (a discriminated union), keeping each field's name and type.
/// Demonstrates quick fixes that reshape the DSL itself rather than just patch identifiers.
module ConvertAction =

    let private header =
        Regex(@"\bRecord\(\s*""([^""]+)""\s*\)\s*\{", RegexOptions.Compiled)

    // `Field("Name", <type expr>)` — the type side is captured verbatim, so both string
    // shorthands (`"int"`) and widget types (`Int()`) survive the conversion.
    let private field =
        Regex(@"\bField\(\s*(""[^""]+""\s*,\s*.+)\)", RegexOptions.Compiled)

    /// 1-based line of the first `Record(…) {` outside a line comment, if any.
    let anchorLine(source: string) : int option =
        source.Replace("\r\n", "\n").Split('\n')
        |> Array.tryFindIndex(fun l ->
            not(l.TrimStart().StartsWith("//", StringComparison.Ordinal))
            && header.IsMatch l)
        |> Option.map(fun i -> i + 1)

    /// The source with the first Record block rewritten as a Union, or None if there is none.
    let convertRecordToUnion(source: string) : string option =
        match anchorLine source with
        | None -> None
        | Some startLine ->
            let lines = source.Replace("\r\n", "\n").Split('\n')
            let out = Array.copy lines
            let start = startLine - 1

            out.[start] <- header.Replace(lines.[start], "Union(\"$1\") {", 1)

            // Walk the block by brace depth, turning each Field into a UnionCase.
            let mutable depth = 0
            let mutable i = start
            let mutable insideBlock = true

            while insideBlock && i < lines.Length do
                let line = lines.[i]

                depth <-
                    depth + Seq.length(Seq.filter ((=) '{') line)
                    - Seq.length(Seq.filter ((=) '}') line)

                if i > start then
                    out.[i] <- field.Replace(line, "UnionCase($1)")

                if depth <= 0 then
                    insideBlock <- false

                i <- i + 1

            Some(String.concat "\n" out)
