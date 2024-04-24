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
                        ParenPat(TuplePat([ ParameterPat("name", String()); ParameterPat("age", Int32()) ]))
                    )
                ) {
                    AutoProperty("Name", Unquoted "name")
                    AutoProperty("Age", Unquoted "age")
                    AutoPropertyGet("A", DoubleQuoted "")
                    AutoPropertyGet("B", DoubleQuoted "").toStatic()

                    AutoProperty("C", DoubleQuoted "")
                        .toPrivate()
                        .xmlDocs([ "Im a private property" ])

                    AutoProperty("D", DoubleQuoted "")
                        .toInternal()
                        .returnType(String())
                        .xmlDocs([ "Im an internal property with a return type" ])

                    AutoProperty("E", DoubleQuoted "")
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
