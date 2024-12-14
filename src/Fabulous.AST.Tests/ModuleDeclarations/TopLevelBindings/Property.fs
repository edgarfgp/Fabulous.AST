namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module PropertyMember =

    [<Fact>]
    let ``Produces PropertiesMembers``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent "string") }).members() {
                    Member(ConstantPat(Constant("this.A")), ConstantExpr(String ""))

                    Member(ConstantPat(Constant("this.C")), ConstantExpr(String "")).toInlined()

                    Member(ConstantPat(Constant("B")), ConstantExpr(String "")).toStatic()

                    Member(ConstantPat(Constant("D")), ConstantExpr(String ""))
                        .toStatic()
                        .toInlined()

                    Member(ConstantPat(Constant("this.E")), ConstantExpr(String ""))
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("this.F")), ConstantExpr(String "")).toInlined()
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("G")), ConstantExpr(String "")).toStatic()
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("H")), ConstantExpr(String ""))
                        .toStatic()
                        .toInlined()
                    |> _.returnType(String())
                }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""
    member inline this.C = ""
    static member B = ""
    static member inline D = ""
    member this.E: string = ""
    member inline this.F: string = ""
    static member G: string = ""
    static member inline H: string = ""

"""

    [<Fact>]
    let ``Produces Properties with Patterns``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                    Member(ConstantPat(Constant("this.A")), ConstantExpr(String ""))

                    Member(ConstantPat(Constant("this.C")), ConstantExpr(String "")).toInlined()

                    Member(ConstantPat(Constant("(|B|_|)")), ConstantExpr(String "")).toStatic()

                    Member(ConstantPat(Constant("D")), ConstantExpr(String ""))
                        .toStatic()
                        .toInlined()

                    Member(ConstantPat(Constant("this.E")), ConstantExpr(String ""))
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("this.F")), ConstantExpr(String "")).toInlined()
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("G")), ConstantExpr(String "")).toStatic()
                    |> _.returnType(String())

                    Member(ConstantPat(Constant("H")), ConstantExpr(String ""))
                        .toStatic()
                        .toInlined()
                    |> _.returnType(String())
                }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""
    member inline this.C = ""
    static member (|B|_|) = ""
    static member inline D = ""
    member this.E: string = ""
    member inline this.F: string = ""
    static member G: string = ""
    static member inline H: string = ""

"""

    [<Fact>]
    let ``Produces a record with property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                    Member(ConstantPat(Constant("this.A")), ConstantExpr(String ""))
                }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""

"""

    [<Fact>]
    let ``Produces a record with static property member``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) }).members() {
                    Member(ConstantPat(Constant("A")), ConstantExpr(String "")).toStatic()
                }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A = ""

"""

    [<Fact>]
    let ``Produces a record with TypeParams and property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    Member(ConstantPat(Constant("this.A")), ConstantExpr(String ""))
                }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A = ""

"""

    [<Fact>]
    let ``Produces a record with TypeParams and static property member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    Member(ConstantPat(Constant("A")), ConstantExpr(String "")).toStatic()
                }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A = ""

"""

    [<Fact>]
    let ``Produces a class with a static and not static member property ``() =

        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name1")), ConstantExpr(String "name"))

                    Member(ConstantPat(Constant("Name2")), ConstantExpr(String "name")).toStatic()
                }
            }
        }
        |> produces
            """
type Person() =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Fact>]
    let ``Produces a generic class with a static and not static member property ``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name1")), ConstantExpr(String "name"))

                    Member(ConstantPat(Constant("Name2")), ConstantExpr(String "name")).toStatic()
                }
                |> _.typeParams(PostfixList([ "'other" ]))
            }
        }
        |> produces
            """
type Person<'other>() =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property with xml comments``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                        .xmlDocs([ "This is a comment" ])
                }
            }
        }
        |> produces
            """
type Person() =
    /// This is a comment
    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property and accessibility controls``() =
        let data =
            [ "Name", AccessControl.Public
              "Age", AccessControl.Private
              "Address", AccessControl.Internal
              "PostalCode", AccessControl.Unknown ]

        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    for name, acc in data do
                        let widget =
                            Member(ConstantPat(Constant($"this.{name}")), ConstantExpr(String "name"))

                        match acc with
                        | AccessControl.Public -> widget.toPublic()
                        | AccessControl.Private -> widget.toPrivate()
                        | AccessControl.Internal -> widget.toInternal()
                        | AccessControl.Unknown -> widget
                }
            }
        }
        |> produces
            """
type Person() =
    member public this.Name = "name"
    member private this.Age = "name"
    member internal this.Address = "name"
    member this.PostalCode = "name"

"""

    [<Fact>]
    let ``Produces a class with a member property and return type``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Int 23))
                    |> _.returnType(LongIdent "int")
                }

            }
        }
        |> produces
            """
type Person() =
    member this.Name: int = 23
"""

    [<Fact>]
    let ``Produces a class with a member property inlined``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                        .toInlined()
                }
            }
        }
        |> produces
            """
type Person() =
    member inline this.Name = "name"

"""

    [<Fact>]
    let ``Produces a class with property member with attributes``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Int 23))
                        .attribute(Attribute "Obsolete")
                }

            }
        }
        |> produces
            """
type Person() =
    [<Obsolete>]
    member this.Name = 23
"""

    [<Fact>]
    let ``Produces a record with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("string")) }).members() {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                }

            }
        }
        |> produces
            """
type Person =
    { Name: string }

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a generic record with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("'other")) })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                }

            }
        }
        |> produces
            """
type Person<'other> =
    { Name: 'other }

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a union with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") }).members() {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                }

            }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a union with a static member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") }).members() {
                    Member(ConstantPat(Constant("Name")), ConstantExpr(String "name"))
                        .toStatic()
                        .toStatic()
                }

            }
        }
        |> produces
            """
type Person =
    | Name

    static member Name = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "name"))
                }

            }
        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a static member property ``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams(PostfixList([ "'other" ]))
                    .members() {
                    Member(ConstantPat(Constant("Name")), ConstantExpr(String "name")).toStatic()
                }

            }
        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    static member Name = "name"

"""
