namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrList =

    [<Fact>]
    let ``let value with a Array expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ArrayExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
                )
            }
        }
        |> produces
            """

let x = [| 1; 2; 3 |]
"""

    [<Fact>]
    let ``let value with a List expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ListExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
                )
            }
        }
        |> produces
            """

let x = [ 1; 2; 3 ]
"""

    [<Fact>]
    let ``let value with a List expression range operator``() =
        Oak() {
            AnonymousModule() {
                ArrayExpr([ TripleNumberIndexRangeExpr("-24.0", "-1.0", "-30.0") ])
                ListExpr([ TripleNumberIndexRangeExpr("-24.0", "-1.0", "-30.0") ])
            }
        }
        |> produces
            """

[| -24.0 .. -1.0 .. -30.0 |]
[ -24.0 .. -1.0 .. -30.0 ]
"""

    [<Fact>]
    let ``Array expression with string literals``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ArrayExpr([ "hello"; "world"; "!" ])) } }
        |> produces
            """

let x = [| hello; world; ! |]
"""

    [<Fact>]
    let ``List expression with string literals``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ListExpr([ "hello"; "world"; "!" ])) } }
        |> produces
            """

let x = [ hello; world; ! ]
"""

    [<Fact>]
    let ``Array expression with constant widgets``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ArrayExpr([ Int 1; Int 2; Int 3 ])) } }
        |> produces
            """

let x = [| 1; 2; 3 |]
"""

    [<Fact>]
    let ``List expression with constant widgets``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ListExpr([ Int 1; Int 2; Int 3 ])) } }
        |> produces
            """

let x = [ 1; 2; 3 ]
"""

    [<Fact>]
    let ``Empty array expression``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), EmptyArrayExpr()) } }
        |> produces
            """

let x = [||]
"""

    [<Fact>]
    let ``Empty list expression``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), EmptyListExpr()) } }
        |> produces
            """

let x = []
"""

    [<Fact>]
    let ``Mixed expressions in array``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ArrayExpr(
                        [ ConstantExpr(Int 1)
                          ConstantExpr(Constant("a"))
                          InfixAppExpr(ConstantExpr(Int 2), "+", ConstantExpr(Int 3)) ]
                    )
                )
            }
        }
        |> produces
            """

let x = [| 1; a; 2 + 3 |]
"""

    [<Fact>]
    let ``Mixed expressions in list``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ListExpr(
                        [ ConstantExpr(Int 1)
                          ConstantExpr(Constant("a"))
                          InfixAppExpr(ConstantExpr(Int 2), "+", ConstantExpr(Int 3)) ]
                    )
                )
            }
        }
        |> produces
            """

let x = [ 1; a; 2 + 3 ]
"""

    [<Fact>]
    let ``Array with nested list``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ArrayExpr([ ConstantExpr(Int 1); ListExpr([ ConstantExpr(Int 2); ConstantExpr(Int 3) ]) ])
                )
            }
        }
        |> produces
            """

let x = [| 1; [ 2; 3 ] |]
"""

    [<Fact>]
    let ``List with nested array``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ListExpr([ ConstantExpr(Int 1); ArrayExpr([ ConstantExpr(Int 2); ConstantExpr(Int 3) ]) ])
                )
            }
        }
        |> produces
            """

let x = [ 1; [| 2; 3 |] ]
"""

    [<Fact>]
    let ``Passing array to a function``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("result")),
                    AppExpr("Array.sum", ArrayExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ]))
                )
            }
        }
        |> produces
            """

let result = Array.sum [| 1; 2; 3 |]
"""

    [<Fact>]
    let ``Passing list to a function``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("result")),
                    AppExpr("List.sum", ListExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ]))
                )
            }
        }
        |> produces
            """

let result = List.sum [ 1; 2; 3 ]
"""
