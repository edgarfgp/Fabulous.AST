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
                Class(
                    "Person",
                    ImplicitConstructor(ParenPat(TuplePat([ ParameterPat(ConstantPat(Constant("name")), String()) ])))
                ) {
                    InheritConstructorUnit(LongIdent "BaseClass")

                    Property(
                        "this.Name",
                        GetterBinding(ConstantExpr(Constant "name")),
                        SetterBinding(NamedPat("value"), ConstantExpr(Constant("()")))
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
