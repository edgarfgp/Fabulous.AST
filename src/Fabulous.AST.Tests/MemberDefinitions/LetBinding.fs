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
                Class("Person") {

                    LetBinding(Value("_name", Quoted "").toMutable())

                    LetBindings(
                        [ Value("_age", Quoted "0").toMutable()
                          Value("_height", Quoted "0.0").toMutable()
                          Value("_weight", Quoted "0.0").toMutable() ]
                    )

                    Property(
                        "this.Name",
                        GetterBinding(Unquoted("_name")),
                        SetterBinding(NamedPat("value"), Unquoted("_name <- value"))
                    )

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
