namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module Function =

    [<Fact>]
    let ``Produces a function with widget parameter``() =
        Oak() {
            AnonymousModule() { Function("x", ParameterPat(ConstantPat(Constant "i")), ConstantExpr(ConstantUnit())) }
        }
        |> producesValid
            """

let x i = ()

"""

    [<Fact>]
    let ``Produces function with summary xml docs``() =
        Oak() { AnonymousModule() { Function("add", [ "a"; "b" ], "a + b").xmlDocs(Summary("This is a comment")) } }
        |> producesValid
            """
/// <summary>
/// This is a comment
/// </summary>
let add a b = a + b
"""

    [<Fact>]
    let ``Produces a function with type params``() =
        Oak() { AnonymousModule() { Function("add", [ "a"; "b" ], "a + b").typeParams(PostfixList(TyparDecl("'a"))) } }
        |> producesValid
            """
let add<'a> a b = a + b
"""

    [<Fact>]
    let ``Produces a function with a body ComputationExpressionStatement``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParameterPat("i"),
                    [ LetOrUseExpr(Value("x", Int(0)))
                      LetOrUseExpr(Value("y", Int(1)))
                      LetOrUseExpr(Function("z", [ ParameterPat("i") ], [ Value("x", Int(0)); Value("y", Int(1)) ]))
                      OtherExpr(AppExpr("z", "i")) ]
                )
            }
        }
        |> produces
            """
let x i =
    let x = 0
    let y = 1

    let z i =
        let x = 0
        let y = 1

    z i
"""

    [<Fact>]
    let ``Produces a function with a body BindingNode expression``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParameterPat("i"),
                    [ Value("x", Int(0))
                      Value("y", Int(1))
                      Function("z", ParameterPat("i"), [ Value("x", Int(0)); Value("y", Int(1)) ]) ]
                )
            }
        }
        |> produces
            """
let x i =
    let x = 0
    let y = 1

    let z i =
        let x = 0
        let y = 1
"""

    [<Fact>]
    let ``Produces a function with a body Expr``() =
        Oak() {
            AnonymousModule() {
                Function("x", ParameterPat("i"), [ AppExpr("a", "i"); AppExpr("b", "i"); AppExpr("c", "i") ])
            }
        }
        |> producesValid
            """
let x i =
    a i
    b i
    c i
"""

    [<Fact>]
    let ``Produces a function with widget parameters``() =
        Oak() {
            AnonymousModule() {
                Function("x", [ ParameterPat(ConstantPat(Constant "i")) ], ConstantExpr(ConstantUnit()))
                Function("x2", ParameterPat(ConstantPat("i")), ConstantExpr("()"))

                Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))

                Function(
                    "y",
                    [ ParameterPat(ConstantPat("i")); ParameterPat(ConstantPat("j")) ],
                    ConstantExpr(ConstantUnit())
                )

                Function("y2", [ ParameterPat("i"); ParameterPat("j") ], ConstantExpr(ConstantUnit()))
                Function("z", [ "i"; "j" ], ConstantExpr(ConstantUnit()))
            }
        }
        |> producesValid
            """

let x i = ()
let x2 i = ()
let add a b = a + b
let y i j = ()
let y2 i j = ()
let z i j = ()

"""

    [<Fact>]
    let ``Produces a function with parameter``() =
        Oak() {
            AnonymousModule() {
                Function("x", "i", ConstantExpr(ConstantUnit()))
                Function("y", "i", ConstantUnit())
                Function("z", "i", "()")
                Function("add", "a b", "a + b")
            }
        }
        |> producesValid
            """

let x i = ()
let y i = ()
let z i = ()
let add a b = a + b

"""

    [<Fact>]
    let ``Produces a function with single tupled parameter``() =
        Oak() { AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())) } }
        |> producesValid
            """
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with single parameter``() =
        Oak() { AnonymousModule() { Function("x", ParenPat(NamedPat("i")), ConstantExpr(ConstantUnit())) } }
        |> producesValid
            """
let x (i) = ()

"""

    [<Fact>]
    let ``Produces a function with single tupled typed parameter``() =
        Oak() {
            AnonymousModule() {
                Function("x", ParenPat(ParameterPat(NamedPat("i"), Int())), ConstantExpr(ConstantUnit()))
                Function("x", ParenPat(ParameterPat("i", Int())), ConstantExpr(ConstantUnit()))
                Function("x", ParenPat(ParameterPat("i", "int")), ConstantExpr(ConstantUnit()))
            }
        }
        |> producesValid
            """
let x (i: int) = ()
let x (i: int) = ()
let x (i: int) = ()

"""

    [<Fact>]
    let ``Produces a function with tupled parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant "i"))
                              ParameterPat(ConstantPat(Constant "j"))
                              ParameterPat(ConstantPat(Constant "k")) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> producesValid
            """
let x (i, j, k) = ()

"""

    [<Fact>]
    let ``Produces a function with tupled parameters using named pat``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(TuplePat([ NamedPat("i"); NamedPat("j"); NamedPat("k") ])),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> producesValid
            """
let x (i, j, k) = ()
"""

    [<Fact>]
    let ``Produces a function with tupled parameters using parameter pat``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("i")), Int())
                              ParameterPat(ConstantPat(Constant "j"), String())
                              ParameterPat(ConstantPat(Constant "k")) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> producesValid
            """
let x (i: int, j: string, k) = ()
"""

    [<Fact>]
    let ``Produces a function with curried parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    LongIdentPat(
                        [ ParameterPat(ConstantPat(Constant "i"))
                          ParameterPat(ConstantPat(Constant "j"))
                          ParameterPat(ConstantPat(Constant "k")) ]
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> producesValid
            """
let x i j k = ()

"""

    [<Fact>]
    let ``Produces a function with tupled typed parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(NamedPat("i"), Int())
                              ParameterPat(NamedPat("j"), String())
                              ParameterPat(NamedPat("k"), Boolean()) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> producesValid
            """
let x (i: int, j: string, k: bool) = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and an attribute``() =
        Oak() {
            AnonymousModule() {
                Function("x", [ NamedPat("i") ], ConstantExpr(ConstantUnit()))
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(String "Use bar instead"))))
            }
        }
        |> producesValid
            """
[<Obsolete("Use bar instead")>]
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and Xml Doc``() =
        Oak() {
            AnonymousModule() {
                Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())).xmlDocs([ "Im a function" ])
            }
        }
        |> producesValid
            """
/// Im a function
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and return type``() =
        Oak() { AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit()), Unit()) } }
        |> producesValid
            """
let x i : unit = ()

"""

    [<Fact>]
    let ``Produces a function with parameters, return type and typeParams ``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "foo",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant "x"), LongIdent "'T")
                              ParameterPat(NamedPat("i"), LongIdent "'U") ]
                        )
                    ),
                    ConstantExpr(ConstantUnit()),
                    Unit()
                )
            }
        }
        |> producesValid
            """
let foo (x: 'T, i: 'U) : unit = ()

"""

    [<Fact>]
    let ``Produces an inlined function with parameters``() =
        Oak() { AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())).toInlined() } }
        |> producesValid
            """

let inline x i = ()

"""

    [<Fact>]
    let ``Produces an function with parameters and constant expr ``() =
        Oak() { AnonymousModule() { Function("add", [ ParameterPat("a"); ParameterPat("b") ], Constant("a + b")) } }
        |> producesValid
            """
let add a b = a + b
"""

    [<Fact>]
    let ``Produces a function with parameters and access controls``() =
        Oak() {
            AnonymousModule() {
                Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())).toPublic()

                Function("y", NamedPat("i"), ConstantExpr(ConstantUnit())).toPrivate()

                Function("z", NamedPat("i"), ConstantExpr(ConstantUnit())).toInternal()
            }
        }
        |> producesValid
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""

    [<Fact>]
    let ``Produces a function with ComputationExpressionStatement``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ]
                )
            }
        }
        |> producesValid
            """
let cylinderVolume radius length = length * pi * radius * radius
"""

    [<Fact>]
    let ``Produces a function with ParameterPat ComputationExpressionStatement``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    ParameterPat "radius",
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
            }
        }
        |> producesValid
            """
let cylinderVolume radius =
    let pi = 3.14159
    length * pi * radius * radius
"""

    [<Fact>]
    let ``yield! multiple functions``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ Function("add", [ "a"; "b" ], "a + b")
                      Function("subtract", [ "a"; "b" ], "a - b")
                      Function("multiply", [ "a"; "b" ], "a * b") ]
            }
        }
        |> producesValid
            """
let add a b = a + b
let subtract a b = a - b
let multiply a b = a * b
"""

    [<Fact>]
    let ``Typed parameters are parenthesized``() =
        Oak() {
            AnonymousModule() {
                Function("f", ParameterPat("path", String()), "path")
                Function("g", [ ParameterPat("a", String()); ParameterPat("b", Int()) ], "a")
                Function("h", [ ParenPat(ParameterPat("a", String())); ParameterPat("b") ], "a")
            }
        }
        |> producesValid
            """
let f (path: string) = path
let g (a: string) (b: int) = a
let h (a: string) b = a
"""

    [<Fact>]
    let ``Tuple and other compound parameters are parenthesized``() =
        Oak() {
            AnonymousModule() {
                Function("fst", TuplePat([ NamedPat("a"); NamedPat("_") ]), "a")
                Function("head", ListConsPat(NamedPat("x"), NamedPat("_")), "x")
            }
        }
        |> producesValid
            """
let fst (a, _) = a
let head (x :: _) = x
"""

    [<Fact>]
    let ``Attributed, union case and type test parameters are parenthesized``() =
        Oak() {
            AnonymousModule() {
                Function("caller", ParameterPat("name").attribute(Attribute("CallerMemberName")), "name")
                Function("unwrap", LongIdentPat("Some", NamedPat("x")), "x")
                Function("isText", IsInstPat(String()), Bool(true))
                Function("ignore2", [ NamedPat("x"); UnitPat(); ParenPat(NamedPat("y")) ], "x")
            }
        }
        |> producesValid
            """
let caller ([<CallerMemberName>] name) = name
let unwrap (Some x) = x
let isText (:? string) = true
let ignore2 x () (y) = x
"""

    [<Fact>]
    let ``Compound patterns wrapped in an untyped ParameterPat are parenthesized``() =
        Oak() {
            AnonymousModule() {
                Function("fst", ParameterPat(TuplePat([ NamedPat("a"); NamedPat("b") ])), "a")
                Function("unwrap", ParameterPat(LongIdentPat("Some", NamedPat("x"))), "x")
                Function("id", ParameterPat(NamedPat("x")), "x")
            }
        }
        |> producesValid
            """
let fst (a, b) = a
let unwrap (Some x) = x
let id x = x
"""

    [<Fact>]
    let ``Active pattern names are not backticked``() =
        Oak() {
            AnonymousModule() {
                Function("(|Even|Odd|)", ParameterPat("n", Int()), "if n % 2 = 0 then Even else Odd")
                Function("(|Int|_|)", ParameterPat("s", String()), "None")
            }
        }
        |> producesValid
            """
let (|Even|Odd|) (n: int) = if n % 2 = 0 then Even else Odd
let (|Int|_|) (s: string) = None
"""

    [<Fact>]
    let ``Recursive functions use let rec``() =
        Oak() {
            AnonymousModule() {
                Function("fact", ParameterPat("n", Int()), "if n <= 1 then 1 else n * fact (n - 1)").toRecursive()

                Function("count", ParameterPat("xs"), "List.length xs").toRecursive().toInlined()
                Value("ones", "1 :: ones").toRecursive()
            }
        }
        |> producesValid
            """
let rec fact (n: int) = if n <= 1 then 1 else n * fact (n - 1)
let rec inline count xs = List.length xs
let rec ones = 1 :: ones
"""
