namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module PropertyGetSet =

    [<Fact>]
    let ``Produces a classes with an auto property``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    Constructor(ParenPat(TuplePat([ ParameterPat("name", String()); ParameterPat("age", Int32()) ])))
                ) {
                    Property(
                        "this.Name",
                        GetterBinding(Unquoted("name")),
                        SetterBinding(NamedPat("value"), Unquoted("()"))
                    )
                }

            }
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name
        with get () = name
        and set value = ()

"""
