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

                    Property(
                        "this.Age",
                        GetterBinding(Unquoted("age")),
                        SetterBinding(NamedPat("value"), ConstantExpr(ConstantUnit()))
                    )
                        .toInlined()

                    Property(
                        "this.Name2",
                        GetterBinding(Unquoted("name")).toInlined(),
                        SetterBinding(NamedPat("value"), Unquoted("()"))
                    )
                        .toPrivate()

                    Property(
                        "this.FirstName",
                        GetterBinding(Unquoted("firstName")),
                        SetterBinding(ParenPat(NamedPat("value")), Unquoted("()"))
                    )

                    Property(
                        "this.LastName",
                        GetterBinding(Unquoted("lastName")).returnType(String()),
                        SetterBinding(ParenPat(ParameterPat("value", String())), ConstantExpr(ConstantUnit()))
                            .returnType(Unit())
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

    member inline this.Age
        with get () = age
        and set value = ()

    member private this.Name2
        with inline get () = name
        and set value = ()

    member this.FirstName
        with get () = firstName
        and set (value) = ()

    member this.LastName
        with get (): string = lastName
        and set (value: string): unit = ()

"""
