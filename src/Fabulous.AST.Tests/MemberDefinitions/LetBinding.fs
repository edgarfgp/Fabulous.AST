namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module LetBinding =

    [<Fact>]
    let ``Produces a classes LetBinding``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {

                    Value(ConstantPat(Constant "_name"), ConstantExpr(String(""))).toMutable()

                    Value(ConstantPat(Constant "_age"), ConstantExpr(String("0"))).toMutable()
                    Value(ConstantPat(Constant "_height"), ConstantExpr(String("0.0"))).toMutable()
                    Value(ConstantPat(Constant "_weight"), ConstantExpr(String("0.0"))).toMutable()

                    Member(
                        "this.Name",
                        Getter(ConstantExpr(Constant "_name")),
                        Setter(NamedPat("value"), ConstantExpr(Constant "_name <- value"))
                    )
                }

            }
        }
        |> producesValid
            """
type Person() =
    let mutable _name = ""
    let mutable _age = "0"
    let mutable _height = "0.0"
    let mutable _weight = "0.0"

    member this.Name
        with get () = _name
        and set value = _name <- value

"""

    [<Fact>]
    let ``Produces a classes LetBindings``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {
                    LetBindings(
                        [ Value(ConstantPat(Constant "_name"), ConstantExpr(String(""))).toMutable()
                          Value(ConstantPat(Constant "_age"), ConstantExpr(String("0"))).toMutable()
                          Value(ConstantPat(Constant "_height"), ConstantExpr(String("0.0"))).toMutable()
                          Value(ConstantPat(Constant "_weight"), ConstantExpr(String("0.0"))).toMutable() ]
                    )

                    LetBinding(Value(ConstantPat(Constant "_age"), ConstantExpr(String("0"))).toMutable())
                }

            }
        }
        |> producesValid
            """
type Person() =
    let mutable _name = ""
    let mutable _age = "0"
    let mutable _height = "0.0"
    let mutable _weight = "0.0"

    let mutable _age = "0"
"""

    [<Fact>]
    let ``toStatic on a class let binding produces static let``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Counter", UnitPat()) {
                    Value("count", Int(0)).toMutable().toStatic()
                    LetBinding(Value("step", Int(1)).toStatic())

                    LetBinding(
                        Function("fact", ParameterPat("n"), "if n <= 1 then 1 else n * fact (n - 1)")
                            .toRecursive()
                            .toStatic()
                    )

                    Member("_.Next", UnitPat(), "count <- count + step; fact count").toStatic()
                }
            }
        }
        |> producesValid
            """
type Counter() =
    static let mutable count = 0
    static let step = 1
    static let rec fact n = if n <= 1 then 1 else n * fact (n - 1)
    static member _.Next() = count <- count + step; fact count
"""

    [<Fact>]
    let ``toAnd continues a let rec group at module level and in a class``() =
        Oak() {
            AnonymousModule() {
                Function("isEven", ParameterPat("n"), "if n = 0 then true else isOdd (n - 1)").toRecursive()

                Function("isOdd", ParameterPat("n"), "if n = 0 then false else isEven (n - 1)").toAnd()

                TypeDefn("Walker", UnitPat()) {
                    Function("a", UnitPat(), "b ()").toRecursive()
                    Function("b", UnitPat(), "a ()").toAnd()
                    Member("_.Run()", "a ()")
                }
            }
        }
        |> producesValid
            """
let rec isEven n = if n = 0 then true else isOdd (n - 1)
and isOdd n = if n = 0 then false else isEven (n - 1)

type Walker() =
    let rec a () = b ()
    and b () = a ()
    member _.Run() = a ()
"""
