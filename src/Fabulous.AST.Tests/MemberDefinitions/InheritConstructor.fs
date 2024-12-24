namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module InheritConstructor =

    [<Fact>]
    let ``Produces a classes InheritConstructor``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(TuplePat([ ParameterPat(ConstantPat(Constant("name")), String()) ]))) {
                    InheritUnit(LongIdent "BaseClass")

                    Member(
                        "this.Name",
                        Getter(ConstantExpr(Constant "name")),
                        Setter(NamedPat("value"), ConstantExpr(Constant("()")))
                    )

                }

            }
        }
        |> produces
            """
type Person(name: string) =
    inherit BaseClass()

    member this.Name
        with get () = name
        and set value = ()

"""
