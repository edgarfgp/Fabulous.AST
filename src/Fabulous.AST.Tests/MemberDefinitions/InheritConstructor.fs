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
                TypeDefn(
                    "Person",
                    ImplicitConstructor(ParenPat(TuplePat([ ParameterPat(ConstantPat(Constant("name")), String()) ])))
                ) {
                    InheritConstructorUnit(LongIdent "BaseClass")

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
