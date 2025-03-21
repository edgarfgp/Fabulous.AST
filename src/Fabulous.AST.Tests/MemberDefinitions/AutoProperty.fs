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
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("name")), String())
                              ParameterPat(ConstantPat(Constant("age")), Int()) ]
                        )
                    )
                ) {
                    MemberVal("Name", ConstantExpr(Constant("name")), true, true)
                        .xmlDocs(Summary("The name of the person"))

                    MemberVal("Age", ConstantExpr(Constant("age")), true, true)
                    MemberVal("A", ConstantExpr(String("")), true)
                    MemberVal("B", ConstantExpr(String("")), true).toStatic()

                    MemberVal("C", ConstantExpr(String("")), true, true)
                        .toPrivate()
                        .xmlDocs([ "Im a private property" ])

                    MemberVal("D", ConstantExpr(String("")), String(), true, true)
                        .toInternal()
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
    /// <summary>
    /// The name of the person
    /// </summary>
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

    [<Fact>]
    let ``public get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X", UnitPat()) {
                    MemberVal("Y", Int(7), Int(), true, true, AccessControl.Public, AccessControl.Private)
                }
            }
        }
        |> produces
            """
type X() =
    member val Y: int = 7 with public get, private set
"""

    [<Fact>]
    let ``plain get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X", UnitPat()) {
                    MemberVal("Y", Int(7), Int(), true, true, setterAccessibility = AccessControl.Private)
                }
            }
        }
        |> produces
            """
type X() =
    member val Y: int = 7 with get, private set
"""
//
// [<Test>]
// let ``plain get, private set`` () =
//     formatSourceString
//         """
// type X() =
//     member val Y: int = 7 with get, private set
// """
//         config
//     |> prepend newline
//     |> should
//         equal
//         """
// type X() =
//     member val Y: int = 7 with get, private set
// """
//
// [<Test>]
// let ``internal get, plain set`` () =
//     formatSourceString
//         """
// type X() =
//     member val Y: int = 7 with internal get,  set
// """
//         config
//     |> prepend newline
//     |> should
//         equal
//         """
// type X() =
//     member val Y: int = 7 with internal get, set
// """
//
// [<Test>]
// let ``public get, private set in signature`` () =
//     formatSignatureString
//         """
// module A
// type X =
//     new: unit -> X
//     member internal Y: int with public get, private set
// """
//         config
//     |> prepend newline
//     |> should
//         equal
//         """
// module A
// type X =
//     new: unit -> X
//     member internal Y: int with public get, private set
// """
//
// [<Test>]
// let ``abstract member with public get, private set`` () =
//     formatSignatureString
//         """
// namespace Meh
// type X =
//     abstract Y: int with public  get,  private set
// """
//         config
//     |> prepend newline
//     |> should
//         equal
//         """
// namespace Meh
// type X =
//     abstract Y: int with public get, private set
// """
