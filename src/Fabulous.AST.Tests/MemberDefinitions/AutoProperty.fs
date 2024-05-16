namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module AutoProperty =

    [<Fact>]
    let ``Produces a classes with an auto property``() =
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
                    AutoPropertyGetSet("Name", ConstantExpr(Constant("name")))
                    AutoPropertyGetSet("Age", ConstantExpr(Constant("age")))
                    AutoPropertyGet("A", ConstantExpr(String("")))
                    AutoPropertyGet("B", ConstantExpr(String(""))).toStatic()

                    AutoPropertyGetSet("C", ConstantExpr(String("")))
                        .toPrivate()
                        .xmlDocs([ "Im a private property" ])

                    AutoPropertyGetSet("D", ConstantExpr(String("")))
                        .toInternal()
                        .returnType(String())
                        .xmlDocs([ "Im an internal property with a return type" ])

                    AutoPropertyGetSet("E", ConstantExpr(String("")))
                        .toPublic()
                        .attribute(Attribute("System.Obsolete"))
                        .xmlDocs([ "Im a public property with an attribute" ])
                }

            }
        }
        |> produces
            """
type Person(name: string, age: int) =
    member val Name = name with get, set
    member val Age = age with get, set
    member val A = "" with get
    static member val B = "" with get
    /// Im a private property
    member val private C = "" with get, set
    /// Im an internal property with a return type
    member val internal D: string = "" with get, set

    /// Im a public property with an attribute
    [<System.Obsolete>]
    member val public E = "" with get, set
"""
