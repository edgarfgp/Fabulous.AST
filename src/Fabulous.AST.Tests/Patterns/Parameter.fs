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

    [<Fact>]
    let ``class with attributed constructor parameter``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Class", Constructor(ParameterPat("c", Int()).attribute(Attribute("Obsolete")))) {
                    Member(
                        "this.First",
                        ParenPat(ParameterPat("a", String()).attribute(Attribute("Obsolete"))),
                        UnitExpr()
                    )
                }
            }
        }
        |> produces
            """
type Class([<Obsolete>] c: int) =
    member this.First([<Obsolete>] a: string) = ()
"""

    [<Fact>]
    let ``class with multiple attributed constructor parameters``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "Class",
                    Constructor(
                        TuplePat(
                            [ ParameterPat("a", Int()).attribute(Attribute("Obsolete"))
                              ParameterPat("b", String()).attribute(Attribute("Required")) ]
                        )
                    )
                ) {
                    Member("this.Value", ConstantExpr(Int(0)))
                }
            }
        }
        |> produces
            """
type Class([<Obsolete>] a: int, [<Required>] b: string) =
    member this.Value = 0
"""

    [<Fact>]
    let ``method with attributed parameter using function type``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Class", UnitPat()) {
                    Member(
                        "this.Second",
                        ParenPat(ParameterPat("a", Funs(String(), Int())).attribute(Attribute("A"))),
                        UnitExpr()
                    )
                }
            }
        }
        |> produces
            """
type Class() =
    member this.Second([<A>] a: string -> int) = ()
"""

    [<Fact>]
    let ``parameter with multiple attributes``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "Class",
                    Constructor(ParameterPat("c", Int()).attributes([ Attribute("Obsolete"); Attribute("Required") ]))
                ) {
                    Member("this.Value", ConstantExpr(Int(0)))
                }
            }
        }
        |> produces
            """
type Class([<Obsolete; Required>] c: int) =
    member this.Value = 0
"""
