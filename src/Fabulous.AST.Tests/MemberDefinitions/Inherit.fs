namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module Inherit =

    [<Fact>]
    let ``Produces a classes Inherit``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    ImplicitConstructor(ParenPat(TuplePat([ ParameterPat(ConstantPat(Constant("name")), String()) ])))
                ) {
                    Inherit(LongIdent "BaseClass()")

                    Property(
                        "this.Name",
                        Getter(ConstantExpr(Constant "name")),
                        Setter(NamedPat("value"), ConstantExpr(Constant "()"))
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
