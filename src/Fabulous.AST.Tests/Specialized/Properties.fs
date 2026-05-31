namespace Fabulous.AST.Tests.Specialized

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit
open FsCheck

open type Ast

/// Property-based tests using FsCheck. These verify invariants that hold across
/// many random inputs, complementing the hand-written example-based tests.
///
/// We use the FsCheck core API directly (not FsCheck.Xunit's [<Property>]) because
/// the pinned FsCheck.Xunit 2.x targets xunit v2, while this project uses xunit.v3.
module Properties =

    /// Render a widget tree to its formatted F# string.
    let private renderModule(decls: WidgetBuilder<Fantomas.Core.SyntaxOak.ModuleDecl> list) =
        let oak =
            Oak() {
                AnonymousModule() {
                    for d in decls do
                        d
                }
            }

        Gen.mkOak oak |> Gen.run

    /// Identifiers that won't trip name validation / backtick escaping.
    let private safeIdentifier =
        Gen.elements [ "x"; "y"; "z"; "value"; "result"; "input"; "output"; "foo"; "bar"; "baz" ]

    /// Generate a non-empty list (FsCheck's default can produce empty lists).
    let private nonEmptyListOf gen =
        gen |> Gen.listOf |> Gen.filter(fun xs -> not(List.isEmpty xs))

    [<Fact>]
    let ``Value bindings produce one let per binding, in declared order``() =
        let prop(names: NonEmptyArray<NonEmptyString>) =
            let identifiers =
                names.Get
                |> Array.map(fun s -> "v_" + System.Text.RegularExpressions.Regex.Replace(s.Get, "[^a-zA-Z0-9_]", "_"))
                |> Array.distinct
                |> Array.truncate 5
                |> Array.toList

            if List.isEmpty identifiers then
                true
            else
                let oak =
                    Oak() {
                        AnonymousModule() {
                            for n in identifiers do
                                Value(n, ConstantExpr(Int 0))
                        }
                    }

                let rendered = Gen.mkOak oak |> Gen.run

                // Each identifier should appear in the output, in the same order.
                let lines = rendered.Split('\n')

                let letLines =
                    lines
                    |> Array.filter(fun l -> l.StartsWith("let "))
                    |> Array.map(fun l ->
                        // "let foo = 0" -> "foo"
                        let s = l.Substring(4).Trim()
                        s.Split([| ' '; '=' |]).[0])
                    |> Array.toList

                letLines = identifiers

        Check.QuickThrowOnFailure prop

    [<Fact>]
    let ``Union with N cases names each case exactly once``() =
        let prop(caseCount: PositiveInt) =
            let n = min caseCount.Get 8

            let caseNames = [ for i in 1..n -> sprintf "Case%d" i ]

            let oak =
                Oak() {
                    AnonymousModule() {
                        Union("U") {
                            for c in caseNames do
                                UnionCase(c)
                        }
                    }
                }

            let rendered = Gen.mkOak oak |> Gen.run

            // Each case name should appear exactly once in the output.
            // (Single-case unions render inline like `type U = | Case1`,
            // multi-case unions break across lines — but each case appears once either way.)
            caseNames
            |> List.forall(fun name ->
                let occurrences =
                    System.Text.RegularExpressions.Regex.Matches(rendered, "\\b" + name + "\\b").Count

                occurrences = 1)

        Check.QuickThrowOnFailure prop

    [<Fact>]
    let ``Record with N fields producesValid N field lines``() =
        let prop(fieldCount: PositiveInt) =
            let n = min fieldCount.Get 8

            let fieldNames = [ for i in 1..n -> sprintf "Field%d" i ]

            let oak =
                Oak() {
                    AnonymousModule() {
                        Record("R") {
                            for fn in fieldNames do
                                Field(fn, Int())
                        }
                    }
                }

            let rendered = Gen.mkOak oak |> Gen.run

            // Each field should appear exactly once.
            fieldNames
            |> List.forall(fun fn ->
                let count =
                    rendered.Split('\n') |> Array.filter(fun l -> l.Contains(fn)) |> Array.length

                count = 1)

        Check.QuickThrowOnFailure prop

    [<Fact>]
    let ``Tuple expression with N items always renders with N-1 commas``() =
        let prop(itemCount: PositiveInt) =
            let n = max 2 (min itemCount.Get 8)

            let items = [ for i in 1..n -> ConstantExpr(Int i) ]

            let oak = Oak() { AnonymousModule() { Value("t", TupleExpr(items)) } }

            let rendered = Gen.mkOak oak |> Gen.run

            // The line with the tuple should have exactly n-1 commas.
            let tupleLine = rendered.Split('\n') |> Array.find(fun l -> l.Contains("let t"))

            let commaCount = tupleLine |> Seq.filter(fun c -> c = ',') |> Seq.length

            commaCount = n - 1

        Check.QuickThrowOnFailure prop

    [<Fact>]
    let ``XmlDocs lines on a binding render as preceding triple-slash comments``() =
        let prop(lineCount: PositiveInt) =
            let n = min lineCount.Get 5

            let docLines = [ for i in 1..n -> sprintf "Doc line %d" i ]

            let oak =
                Oak() { AnonymousModule() { (Value("x", ConstantExpr(Int 0))).xmlDocs(docLines) } }

            let rendered = Gen.mkOak oak |> Gen.run

            // Output should contain exactly N "///" prefixed lines.
            let docLineCount =
                rendered.Split('\n')
                |> Array.filter(fun l -> l.TrimStart().StartsWith("///"))
                |> Array.length

            docLineCount = n

        Check.QuickThrowOnFailure prop
