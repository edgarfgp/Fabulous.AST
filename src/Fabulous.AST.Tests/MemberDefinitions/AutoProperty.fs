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
                TypeDefn(
                    "Person",
                    Constructor(
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("name")), String())
                                  ParameterPat(ConstantPat(Constant("age")), Int()) ]
                            )
                        )
                    )
                ) {
                    MemberVal("Name", ConstantExpr(Constant("name")), true, true)
                    MemberVal("Age", ConstantExpr(Constant("age")), true, true)
                    MemberVal("A", ConstantExpr(String("")), true)
                    MemberVal("B", ConstantExpr(String("")), true).toStatic()

                    MemberVal("C", ConstantExpr(String("")), true, true)
                        .toPrivate()
                        .xmlDocs([ "Im a private property" ])

                    MemberVal("D", ConstantExpr(String("")), true, true)
                        .toInternal()
                        .returnType(String())
                        .xmlDocs([ "Im an internal property with a return type" ])

                    MemberVal("E", ConstantExpr(String("")), true, true)
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
