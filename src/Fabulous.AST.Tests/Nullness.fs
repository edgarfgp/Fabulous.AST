namespace Fabulous.AST.Tests

open Fabulous.AST

open Xunit

open type Ast

module NullnessTests =
    [<Fact>]
    let ``du case of string or null``() =
        Oak() { AnonymousModule() { Union("DU") { UnionCase("MyCase", Field(Paren(TypeOrNull(String())))) } } }
        |> producesValid
            """
type DU = MyCase of (string | null)
"""

    [<Fact>]
    let ``or null pattern``() =
        Oak() {
            AnonymousModule() { MatchExpr("x", [ MatchClauseExpr(OrPat(IsInstPat(String()), NullPat()), UnitExpr()) ]) }
        }
        |> producesValid
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
        |> producesValid
            """
type C<'T when 'T: not null> = class end
"""

    [<Fact>]
    let ``multiple or type``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "myFunc",
                    ParenPat(OrPat(String("abc"), OrPat(ParameterPat(String(""), TypeOrNull(String())), String("123")))),
                    Int(15)
                )
            }
        }
        |> producesValid
            """
let myFunc ("abc" | "": string | null | "123") = 15
"""

    [<Fact>]
    let ``null type constraint``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "myFunc",
                    UnitPat(),
                    NullExpr(),
                    WithGlobal("'T", [ ConstraintNotStruct("'T"); ConstraintSingle("'T", "null") ])
                )
            }
        }
        |> producesValid
            """
let myFunc () : 'T when 'T: not struct and 'T: null = null
"""

    [<Fact>]
    let ``not null type constraint``() =
        Oak() {
            AnonymousModule() {
                Function("myFunc", ParenPat(ParameterPat("x", WithGlobal("'T", WhereNotSupportsNull("'T")))), Int(42))
            }
        }
        |> producesValid
            """
let myFunc (x: 'T when 'T: not null) = 42
"""
