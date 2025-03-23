namespace Fabulous.AST.Tests

open Fabulous.AST

open Xunit

open type Ast

module NullnessTests =
    [<Fact>]
    let ``du case of string or null``() =
        Oak() { AnonymousModule() { Union("DU") { UnionCase("MyCase", Field(Paren(Nullness(String())))) } } }
        |> produces
            """
type DU = MyCase of (string | null)
"""

    [<Fact>]
    let ``or null pattern``() =
        Oak() {
            AnonymousModule() { MatchExpr("x", [ MatchClauseExpr(OrPat(IsInstPat(String()), NullPat()), UnitExpr()) ]) }
        }
        |> produces
            """
match x with
| :? string
| null -> ()
"""

    [<Fact>]
    let ``not null in type constraints``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("C") { () }
                |> _.typeParams(PostfixList(TyparDecl("'T"), WhereNotSupportsNull("'T")))
            }
        }
        |> produces
            """
type C<'T when 'T: not null> = class end
"""

// [<Test>]
//  let ``multiple or type`` () =
//      formatSourceString
//          """
//  let myFunc ("abc" | "" : string | null | "123") = 15
//  """
//          config
//      |> prepend newline
//      |> should
//          equal
//          """
//  let myFunc ("abc" | "": string | null | "123") = 15
//  """
//
//  [<Test>]
//  let ``null type constraint`` () =
//      formatSourceString
//          """
//  let myFunc() : 'T when 'T : not struct and 'T:null = null
//  """
//          config
//      |> prepend newline
//      |> should
//          equal
//          """
//  let myFunc () : 'T when 'T: not struct and 'T: null = null
//  """
//
//  [<Test>]
//  let ``not null type constraint`` () =
//      formatSourceString
//          """
//  let myFunc (x: 'T when 'T: not null) = 42
//  """
//          config
//      |> prepend newline
//      |> should
//          equal
//          """
//  let myFunc (x: 'T when 'T: not null) = 42
//  """
//
