namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module PropertyGetSet =

    [<Fact>]
    let ``Produces a classes with get set members``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    ImplicitConstructor(
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("name")), String())
                                  ParameterPat(ConstantPat(Constant("age")), Int()) ]
                            )
                        )
                    )
                ) {
                    Property(
                        "this.Name",
                        GetterBinding(ConstantExpr(Constant("name"))),
                        SetterBinding(NamedPat("value"), ConstantExpr(Constant("()")))
                    )

                    Property(
                        "this.Age",
                        GetterBinding(ConstantExpr(Constant("age"))),
                        SetterBinding(NamedPat("value"), ConstantExpr(ConstantUnit()))
                    )
                        .toInlined()

                    Property(
                        "this.Name2",
                        GetterBinding(ConstantExpr(Constant("name"))).toInlined(),
                        SetterBinding(NamedPat("value"), ConstantExpr(Constant("()")))
                    )
                        .toPrivate()

                    Property(
                        "this.FirstName",
                        GetterBinding(ConstantExpr(Constant("firstName"))),
                        SetterBinding(ParenPat(NamedPat("value")), ConstantExpr(Constant("()")))
                    )

                    Property(
                        "this.LastName",
                        GetterBinding(ConstantExpr(Constant("lastName"))).returnType(String()),
                        SetterBinding(
                            ParenPat(ParameterPat(ConstantPat(Constant("value")), String())),
                            ConstantExpr(ConstantUnit())
                        )
                            .returnType(Unit())
                    )

                    Property(
                        "this.Item",
                        GetterBinding(
                            ParenPat("index"),
                            IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")))
                        ),
                        SetterBinding(
                            [ NamedPat("index"); NamedPat("value") ],
                            SetExpr(
                                IndexWithoutDotExpr(ConstantExpr(Constant("ordinals")), Constant("index")),
                                ConstantExpr(Constant("value"))
                            )
                        )
                    )
                }

            }
        }
        |> produces
            """
type Person(name: string, age: int) =
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

    member this.Item
        with get (index) = ordinals[index]
        and set index value = ordinals[index] <- value

"""
