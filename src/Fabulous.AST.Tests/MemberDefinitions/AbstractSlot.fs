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
                    Abstract("Area", Float(), true)
                    Abstract("Area1", LongIdent "float", true)

                    Abstract("Area2", Float(), hasSetter = true)
                    Abstract("Area3", LongIdent "float", hasSetter = true)

                    Abstract("Area4", Float(), true, true)
                    Abstract("Area5", LongIdent "float", true, true)

                    Abstract("Pi", Float())
                    Abstract("Pi2", LongIdent "float")

                    Abstract("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
                    Abstract("Add2", [ Int(); Int() ], Int(), true)

                    Abstract("Add3", [ (Some "a", Int()); (Some "b", Int()) ], Int(), true)

                    Abstract(
                        "Add4",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int",
                        true
                    )

                    Abstract("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    Abstract("Add6", [ Int(); Int() ], Int())

                    Abstract("Add7", [ (Some "a", Int()); (Some "b", Int()) ], Int())

                    Abstract("Add8", [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ], LongIdent "int")

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
                    Abstract("Area", [ LongIdent "'a" ], Float()).typeParams(PostfixList([ "'a" ]))

                    AbstractMember("Area1", [ LongIdent "'b" ], Float())
                        .typeParams(PostfixList([ "'b" ]))

                }
            }
        }
        |> produces
            """
type Meh =
    abstract Area<'a> : 'a -> float
    abstract member Area1<'b> : 'b -> float
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

                    AbstractMember("Add3", [ (Some "a", Int()); (Some "b", Int()) ], Int(), true)

                    AbstractMember(
                        "Add4",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int",
                        true
                    )

                    AbstractMember("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractMember("Add6", [ Int(); Int() ], Int())

                    AbstractMember("Add7", [ (Some "a", Int()); (Some "b", Int()) ], Int())

                    AbstractMember(
                        "Add8",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int"
                    )

                }
            }
        }
        |> produces
            """
type Meh =
    abstract member Area: float with get
    abstract member Area1: float with get
    abstract member Area2: float with set
    abstract member Area3: float with set
    abstract member Area4: float with get, set
    abstract member Area5: float with get, set
    abstract member Pi: float
    abstract member Pi2: float
    abstract member Add: int * int -> int
    abstract member Add2: int * int -> int
    abstract member Add3: a: int * b: int -> int
    abstract member Add4: a: int * b: int -> int
    abstract member Add5: int -> int -> int
    abstract member Add6: int -> int -> int
    abstract member Add7: a: int -> b: int -> int
    abstract member Add8: a: int -> b: int -> int

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

                    AbstractMember("Add3", [ (Some "a", Int()); (Some "b", Int()) ], Int(), true)

                    AbstractMember(
                        "Add4",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int",
                        true
                    )

                    AbstractMember("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractMember("Add6", [ Int(); Int() ], Int())

                    AbstractMember("Add7", [ (Some "a", Int()); (Some "b", Int()) ], Int())

                    AbstractMember(
                        "Add8",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int"
                    )

                }
                |> _.typeParams(PostfixList([ "'other"; "'another" ]))
            }
        }
        |> produces
            """
type Meh<'other, 'another> =
    abstract member Area: float with get
    abstract member Area1: float with get
    abstract member Area2: float with set
    abstract member Area3: float with set
    abstract member Area4: float with get, set
    abstract member Area5: float with get, set
    abstract member Pi: float
    abstract member Pi2: float
    abstract member Add: int * int -> int
    abstract member Add2: int * int -> int
    abstract member Add3: a: int * b: int -> int
    abstract member Add4: a: int * b: int -> int
    abstract member Add5: int -> int -> int
    abstract member Add6: int -> int -> int
    abstract member Add7: a: int -> b: int -> int
    abstract member Add8: a: int -> b: int -> int

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
    abstract member ClientInfo1: {| Name: string; Version: string option |}

    abstract member ClientInfo2:
        {| Name: string
           Version: string option |}

    abstract member ClientInfo3:
        {| Name: string
           Version: string option |}

    abstract member ClientInfo4:
        {| Name: string
           Version: string option |}

    abstract member ClientInfo4:
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
    abstract member ClientInfo1:
        {| Name: string
           Version: string option |}

    abstract member ClientInfo2:
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
    abstract member ClientInfo1:
        U2<
            {| Notebook: U2<string, NotebookDocumentFilter>
               Cells: {| Language: string |}[] option |},
            {| Notebook: U2<string, NotebookDocumentFilter> option
               Cells: {| Language: string |}[] |}
         >[]
"""
