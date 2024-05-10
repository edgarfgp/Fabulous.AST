namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Parameter =

    [<Fact>]
    let ``let value with a Parameter pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ParameterPat(NamedPat("a")), ConstantExpr(Int(12)))
                Value("a", ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let a = 12
let a = 12
"""

    [<Fact>]
    let ``let value with a typed Parameter pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ParameterPat(NamedPat("a"), String()), ConstantExpr(Int(12)))
                Value(ParameterPat("b", String()), ConstantExpr(Int(12)))
                Value(ParameterPat("c", "string"), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let a: string = 12
let b: string = 12
let c: string = 12
"""

    [<Fact>]
    let ``let value with a Parameter string pattern``() =
        Oak() { AnonymousModule() { Value(ParameterPat(ConstantPat(Constant "a")), ConstantExpr(Int(12))) } }
        |> produces
            """
let a = 12
"""

    [<Fact>]
    let ``let value with a typed Parameter string pattern``() =
        Oak() {
            AnonymousModule() {
                Value(ParameterPat(ConstantPat(Constant("a")), LongIdent "string"), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """
let a: string = 12
"""
