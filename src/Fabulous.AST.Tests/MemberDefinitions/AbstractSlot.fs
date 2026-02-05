namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module AbstractMembers =

    [<Fact>]
    let ``Produces a classes with a abstracts``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Meh") {
                    AbstractMember("Area", Float(), true)

                    AbstractMember("Area1", LongIdent "float", true)

                    AbstractMember("Area2", Float(), hasSetter = true)
                    AbstractMember("Area3", LongIdent "float", hasSetter = true)

                    AbstractMember("Area4", Float(), true, true)
                    AbstractMember("Area5", LongIdent "float", true, true)

                    AbstractMember("Pi", Float())
                    AbstractMember("Pi2", LongIdent "float")

                    AbstractMember("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", isTupled = true)
                    AbstractMember("Add2", [ Int(); Int() ], Int(), true)

                    AbstractMember("Add3", [ ("a", Int()); ("b", Int()) ], Int(), true)

                    AbstractMember("Add4", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int", true)

                    AbstractMember("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractMember("Add6", [ Int(); Int() ], Int())

                    AbstractMember("Add7", [ ("a", Int()); ("b", Int()) ], Int())

                    AbstractMember("Add8", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int")

                }
            }
        }
        |> produces
            """
type Meh =
    abstract Area: float with get
    abstract Area1: float with get
    abstract Area2: float with set
    abstract Area3: float with set
    abstract Area4: float with get, set
    abstract Area5: float with get, set
    abstract Pi: float
    abstract Pi2: float
    abstract Add: int * int -> int
    abstract Add2: int * int -> int
    abstract Add3: a: int * b: int -> int
    abstract Add4: a: int * b: int -> int
    abstract Add5: int -> int -> int
    abstract Add6: int -> int -> int
    abstract Add7: a: int -> b: int -> int
    abstract Add8: a: int -> b: int -> int
"""

    [<Fact>]
    let ``Produces a abstract members with type params``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Meh") {
                    AbstractMember("Area", [ LongIdent "'a" ], Float())
                        .typeParams(PostfixList([ "'a" ]))
                        .attributes([ Attribute("Obsolete") ])

                    AbstractMember("Area1", [ LongIdent "'b" ], Float()).typeParams(PostfixList([ "'b" ]))

                }
            }
        }
        |> produces
            """
type Meh =
    [<Obsolete>]
    abstract Area<'a> : 'a -> float

    abstract Area1<'b> : 'b -> float
"""

    [<Fact>]
    let ``Produces a classes with a abstract members``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Meh") {
                    AbstractMember("Area", Float(), true)
                    AbstractMember("Area1", LongIdent "float", true)

                    AbstractMember("Area2", Float(), hasSetter = true)
                    AbstractMember("Area3", LongIdent "float", hasSetter = true)

                    AbstractMember("Area4", Float(), true, true)
                    AbstractMember("Area5", LongIdent "float", true, true)

                    AbstractMember("Pi", Float())
                    AbstractMember("Pi2", LongIdent "float")

                    AbstractMember("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
                    AbstractMember("Add2", [ Int(); Int() ], Int(), true)

                    AbstractMember("Add3", [ ("a", Int()); ("b", Int()) ], Int(), true)

                    AbstractMember("Add4", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int", true)

                    AbstractMember("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractMember("Add6", [ Int(); Int() ], Int())

                    AbstractMember("Add7", [ ("a", Int()); ("b", Int()) ], Int())

                    AbstractMember("Add8", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int")

                    AbstractMember("Add9", [ Int(); Int() ], "int")

                }
            }
        }
        |> produces
            """
type Meh =
    abstract Area: float with get
    abstract Area1: float with get
    abstract Area2: float with set
    abstract Area3: float with set
    abstract Area4: float with get, set
    abstract Area5: float with get, set
    abstract Pi: float
    abstract Pi2: float
    abstract Add: int * int -> int
    abstract Add2: int * int -> int
    abstract Add3: a: int * b: int -> int
    abstract Add4: a: int * b: int -> int
    abstract Add5: int -> int -> int
    abstract Add6: int -> int -> int
    abstract Add7: a: int -> b: int -> int
    abstract Add8: a: int -> b: int -> int
    abstract Add9: int -> int -> int

"""

    [<Fact>]
    let ``Produces a genetic interface with multiple abstract members types``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Meh") {
                    AbstractMember("Area", Float(), true)
                    AbstractMember("Area1", LongIdent "float", true)

                    AbstractMember("Area2", Float(), hasSetter = true)
                    AbstractMember("Area3", LongIdent "float", hasSetter = true)

                    AbstractMember("Area4", Float(), true, true)
                    AbstractMember("Area5", LongIdent "float", true, true)

                    AbstractMember("Pi", Float())
                    AbstractMember("Pi2", LongIdent "float")

                    AbstractMember("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
                    AbstractMember("Add2", [ Int(); Int() ], Int(), true)

                    AbstractMember("Add3", [ ("a", Int()); ("b", Int()) ], Int(), true)

                    AbstractMember("Add4", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int", true)

                    AbstractMember("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractMember("Add6", [ Int(); Int() ], Int())

                    AbstractMember("Add7", [ ("a", Int()); ("b", Int()) ], Int())

                    AbstractMember("Add8", [ ("a", LongIdent "int"); ("b", LongIdent "int") ], LongIdent "int")

                }
                |> _.typeParams(PostfixList([ "'other"; "'another" ]))
            }
        }
        |> produces
            """
type Meh<'other, 'another> =
    abstract Area: float with get
    abstract Area1: float with get
    abstract Area2: float with set
    abstract Area3: float with set
    abstract Area4: float with get, set
    abstract Area5: float with get, set
    abstract Pi: float
    abstract Pi2: float
    abstract Add: int * int -> int
    abstract Add2: int * int -> int
    abstract Add3: a: int * b: int -> int
    abstract Add4: a: int * b: int -> int
    abstract Add5: int -> int -> int
    abstract Add6: int -> int -> int
    abstract Add7: a: int -> b: int -> int
    abstract Add8: a: int -> b: int -> int

"""

    [<Fact>]
    let ``Produces an inheritance of an interface with abstract properties``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMeh") {
                    Inherit("IFoo")

                    AbstractMember("ClientInfo1", "{| Name: string; Version: string option |}")
                    AbstractMember("ClientInfo2", AnonRecord([ ("Name", "string"); ("Version", "string option") ]))

                    AbstractMember(
                        "ClientInfo3",
                        AnonRecord([ ("Name", String()); ("Version", LongIdent("string option")) ])
                    )

                    AbstractMember(
                        "ClientInfo4",
                        AnonRecord([ ("Name", String()); ("Version", AppPostfix(String(), "option")) ])
                    )

                    AbstractMember(
                        "ClientInfo4",
                        AppPostfix(
                            AnonRecord([ ("Name", String()); ("Version", AppPostfix(String(), "option")) ]),
                            "option"
                        )
                    )
                }
            }
        }
        |> produces
            """
type IMeh =
    inherit IFoo
    abstract ClientInfo1: {| Name: string; Version: string option |}

    abstract ClientInfo2:
        {| Name: string
           Version: string option |}

    abstract ClientInfo3:
        {| Name: string
           Version: string option |}

    abstract ClientInfo4:
        {| Name: string
           Version: string option |}

    abstract ClientInfo4:
        {| Name: string
           Version: string option |} option
"""

    [<Fact>]
    let ``Produces an inheritance of an interface with abstract  option widget``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMeh") {
                    AbstractMember(
                        "ClientInfo1",
                        AnonRecord([ ("Name", String()); ("Version", OptionPostfix(String())) ])
                    )

                    AbstractMember(
                        "ClientInfo2",
                        OptionPostfix(AnonRecord([ ("Name", String()); ("Version", OptionPostfix(String())) ]))
                    )
                }
            }
        }
        |> produces
            """
type IMeh =
    abstract ClientInfo1:
        {| Name: string
           Version: string option |}

    abstract ClientInfo2:
        {| Name: string
           Version: string option |} option
"""

    [<Fact>]
    let ``Produces an inheritance of an interface with abstract member, nested anon records``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMeh") {
                    AbstractMember(
                        "ClientInfo1",
                        Array(
                            AppPrefix(
                                "U2",
                                [ AnonRecord(
                                      [ ("Notebook", AppPrefix("U2", [ String(); LongIdent("NotebookDocumentFilter") ]))
                                        ("Cells", OptionPostfix(Array(AnonRecord([ ("Language", String()) ])))) ]
                                  )

                                  AnonRecord(
                                      [ ("Notebook",
                                         OptionPostfix(
                                             AppPrefix("U2", [ String(); LongIdent("NotebookDocumentFilter") ])
                                         ))
                                        ("Cells", Array(AnonRecord([ ("Language", String()) ]))) ]
                                  ) ]
                            )
                        )
                    )
                }
            }
        }
        |> produces
            """
type IMeh =
    abstract ClientInfo1:
        U2<
            {| Notebook: U2<string, NotebookDocumentFilter>
               Cells: {| Language: string |}[] option |},
            {| Notebook: U2<string, NotebookDocumentFilter> option
               Cells: {| Language: string |}[] |}
         >[]
"""

    [<Fact>]
    let ``Abstract member with public get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Public, AccessControl.Private) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with public get, private set
"""

    [<Fact>]
    let ``Abstract member with plain get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, setterAccessibility = AccessControl.Private) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with get, private set
"""

    [<Fact>]
    let ``Abstract member with internal get, plain set``() =
        Oak() { AnonymousModule() { TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Internal) } } }
        |> produces
            """
type X =
    abstract Y: int with internal get, set
"""

    [<Fact>]
    let ``Abstract member with public get, internal set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Public, AccessControl.Internal) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with public get, internal set
"""

    [<Fact>]
    let ``Abstract member with internal get, internal set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Internal, AccessControl.Internal) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with internal get, internal set
"""

    [<Fact>]
    let ``Abstract member with public get, public set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Public, AccessControl.Public) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with public get, public set
"""

    [<Fact>]
    let ``Abstract member with plain get, plain set``() =
        Oak() { AnonymousModule() { TypeDefn("X") { AbstractMember("Y", Int(), true, true) } } }
        |> produces
            """
type X =
    abstract Y: int with get, set
"""

    [<Fact>]
    let ``Abstract member with internal get, public set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Internal, AccessControl.Public) }
            }
        }
        |> produces
            """
type X =
    abstract Y: int with internal get, public set
"""

    [<Fact>]
    let ``Abstract member with public get, plain set``() =
        Oak() { AnonymousModule() { TypeDefn("X") { AbstractMember("Y", Int(), true, true, AccessControl.Public) } } }
        |> produces
            """
type X =
    abstract Y: int with public get, set
"""

    [<Fact>]
    let ``Abstract member parameters with internal get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Private
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Private
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with internal get, private set
    abstract Add: a: int * b: int -> int with internal get, private set
"""

    [<Fact>]
    let ``Abstract member parameters with public get, private set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Private
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Private
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with public get, private set
    abstract Add: a: int * b: int -> int with public get, private set
"""

    [<Fact>]
    let ``Abstract member parameters with public get, public set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Public
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Public
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with public get, public set
    abstract Add: a: int * b: int -> int with public get, public set
"""

    [<Fact>]
    let ``Abstract member parameters with internal get, internal set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Internal
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Internal
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with internal get, internal set
    abstract Add: a: int * b: int -> int with internal get, internal set
"""

    [<Fact>]
    let ``Abstract member parameters with public get, internal set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Internal
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Public,
                        AccessControl.Internal
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with public get, internal set
    abstract Add: a: int * b: int -> int with public get, internal set
"""

    [<Fact>]
    let ``Abstract member parameters with internal get, public set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Public
                    )

                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        true,
                        true,
                        true,
                        AccessControl.Internal,
                        AccessControl.Public
                    )

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with internal get, public set
    abstract Add: a: int * b: int -> int with internal get, public set
"""

    [<Fact>]
    let ``Abstract member parameters with public get, plain set``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember(
                        "Add",
                        [ ("a", Int()); ("b", Int()) ],
                        Int(),
                        false,
                        true,
                        true,
                        AccessControl.Public
                    )

                    AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int(), true, true, true, AccessControl.Public)

                }
            }
        }
        |> produces
            """
type X =
    abstract Add: a: int -> b: int -> int with public get, set
    abstract Add: a: int * b: int -> int with public get, set
"""

    [<Fact>]
    let ``Abstract member with static modifier``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int(), false).toStatic()

                    AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int(), true).toStatic()

                }
            }
        }
        |> produces
            """
type X =
    static abstract Add: a: int -> b: int -> int
    static abstract Add: a: int * b: int -> int
"""

    [<Fact>]
    let ``Abstract member with xml documentation``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int(), false)
                        .xmlDocs([ "<summary>"; "Summary"; "</summary>" ])

                    AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int(), true)
                        .xmlDocs([ "<summary>"; "Second Summary"; "</summary>" ])

                }
            }
        }
        |> produces
            """
type X =
    /// <summary>
    /// Summary
    /// </summary>
    abstract Add: a: int -> b: int -> int
    /// <summary>
    /// Second Summary
    /// </summary>
    abstract Add: a: int * b: int -> int
"""
