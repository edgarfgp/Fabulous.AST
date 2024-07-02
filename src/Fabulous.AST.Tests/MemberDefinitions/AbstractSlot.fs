namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module AbstractMembers =

    [<Fact>]
    let ``Produces a classes with a abstract members``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Meh") {
                    AbstractSlot("Area", Float(), true)
                    AbstractSlot("Area1", LongIdent "float", true)

                    AbstractSlot("Area2", Float(), hasSetter = true)
                    AbstractSlot("Area3", LongIdent "float", hasSetter = true)

                    AbstractSlot("Area4", Float(), true, true)
                    AbstractSlot("Area5", LongIdent "float", true, true)

                    AbstractSlot("Pi", Float())
                    AbstractSlot("Pi2", LongIdent "float")

                    AbstractSlot("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
                    AbstractSlot("Add2", [ Int(); Int() ], Int(), true)

                    AbstractSlot("Add3", [ (Some "a", Int()); (Some "b", Int()) ], Int(), true)

                    AbstractSlot(
                        "Add4",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int",
                        true
                    )

                    AbstractSlot("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractSlot("Add6", [ Int(); Int() ], Int())

                    AbstractSlot("Add7", [ (Some "a", Int()); (Some "b", Int()) ], Int())

                    AbstractSlot("Add8", [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ], LongIdent "int")

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
                    AbstractSlot("Area", Float(), true)
                    AbstractSlot("Area1", LongIdent "float", true)

                    AbstractSlot("Area2", Float(), hasSetter = true)
                    AbstractSlot("Area3", LongIdent "float", hasSetter = true)

                    AbstractSlot("Area4", Float(), true, true)
                    AbstractSlot("Area5", LongIdent "float", true, true)

                    AbstractSlot("Pi", Float())
                    AbstractSlot("Pi2", LongIdent "float")

                    AbstractSlot("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
                    AbstractSlot("Add2", [ Int(); Int() ], Int(), true)

                    AbstractSlot("Add3", [ (Some "a", Int()); (Some "b", Int()) ], Int(), true)

                    AbstractSlot(
                        "Add4",
                        [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ],
                        LongIdent "int",
                        true
                    )

                    AbstractSlot("Add5", [ LongIdent "int"; LongIdent "int" ], LongIdent "int")
                    AbstractSlot("Add6", [ Int(); Int() ], Int())

                    AbstractSlot("Add7", [ (Some "a", Int()); (Some "b", Int()) ], Int())

                    AbstractSlot("Add8", [ (Some "a", LongIdent "int"); (Some "b", LongIdent "int") ], LongIdent "int")

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

                    AbstractSlot("ClientInfo1", "{| Name: string; Version: string option |}")
                    AbstractSlot("ClientInfo2", AnonRecord([ ("Name", "string"); ("Version", "string option") ]))

                    AbstractSlot(
                        "ClientInfo3",
                        AnonRecord([ ("Name", String()); ("Version", LongIdent("string option")) ])
                    )

                    AbstractSlot(
                        "ClientInfo4",
                        AnonRecord([ ("Name", String()); ("Version", AppPostfix(String(), "option")) ])
                    )

                    AbstractSlot(
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
                    AbstractSlot(
                        "ClientInfo1",
                        AnonRecord([ ("Name", String()); ("Version", OptionPostfix(String())) ])
                    )

                    AbstractSlot(
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
                    AbstractSlot(
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
