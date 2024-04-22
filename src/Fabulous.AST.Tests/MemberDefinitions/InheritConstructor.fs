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
                Class("Person", Constructor(ParenPat(TuplePat([ ParameterPat("name", String()) ])))) {
                    InheritConstructorUnit(Unquoted "BaseClass")

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
type Person(name: string) =
    inherit BaseClass()

    member this.Name
        with get () = name
        and set value = ()

"""
