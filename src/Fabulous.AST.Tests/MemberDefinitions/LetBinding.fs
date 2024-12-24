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

                    LetBinding(Value(ConstantPat(Constant "_name"), ConstantExpr(String(""))).toMutable())

                    LetBindings(
                        [ Value(ConstantPat(Constant "_age"), ConstantExpr(String("0"))).toMutable()
                          Value(ConstantPat(Constant "_height"), ConstantExpr(String("0.0"))).toMutable()
                          Value(ConstantPat(Constant "_weight"), ConstantExpr(String("0.0"))).toMutable() ]
                    )

                    Member(
                        "this.Name",
                        Getter(ConstantExpr(Constant "_name")),
                        Setter(NamedPat("value"), ConstantExpr(Constant "_name <- value"))
                    )
                        .getter(Getter(ConstantExpr(Constant "_name")))
                        .setter(Setter(NamedPat("value"), ConstantExpr(Constant "_name <- value")))

                }

            }
        }
        |> produces
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
