namespace Fabulous.AST.Tests.LetBindings

open Fantomas.Core.SyntaxOak
open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Value =
    [<Test>]
    let ``Top level binding using Value widget`` () =
        AnonymousModule() { Value("x", "12") }
        |> produces
            """

let x = 12

"""

    [<Test>]
    let ``Top level binding using Value widget inlined`` () =
        AnonymousModule() { Value("x", "12").isInlined() }
        |> produces
            """

let inline x = 12

"""

    [<Test>]
    let ``Top level private binding using Value widget`` () =
        AnonymousModule() { Value("x", "12").accessibility(AccessControl.Private) }
        |> produces
            """

let private x = 12

"""

    [<Test>]
    let ``Top level internal binding using Value widget`` () =
        AnonymousModule() { Value("x", "12").accessibility(AccessControl.Internal) }
        |> produces
            """

let internal x = 12

"""

    [<Test>]
    let ``Top level binding using Value widget with a single xml doc`` () =
        AnonymousModule() { Value("x", "12").xmlDocs([ "/// This is a comment" ]) }
        |> produces
            """

/// This is a comment
let x = 12

"""

    [<Test>]
    let ``Top level binding using Value widget with multiline xml doc`` () =
        AnonymousModule() {
            Value("x", "12")
                .xmlDocs(
                    [ "/// This is a fist comment"
                      "/// This is a second comment"
                      "/// This is a third comment" ]
                )

        }
        |> produces
            """

/// This is a fist comment
/// This is a second comment
/// This is a third comment
let x = 12

"""

    [<Test>]
    let ``Top level binding using Value widget with a single attribute`` () =
        AnonymousModule() {
            Value("x", "12").attributes([ "Obsolete" ])

        }
        |> produces
            """
[<Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Top level binding using Value widget with a multiple attributes`` () =
        AnonymousModule() { Value("x", "12").attributes([ "EditorBrowsable"; "Obsolete" ]) }
        |> produces
            """
            
[<EditorBrowsable; Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Top level let binding mutable`` () =
        AnonymousModule() { Value("x", "12").isMutable() }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Top level binding using BindingNode`` () =
        AnonymousModule() { BindingNode.Create("x", "12") }
        |> produces
            """
        
let x = 12

"""

    [<Test>]
    let ``Top level typed binding using BindingNode`` () =
        AnonymousModule() { BindingNode.Create("x", "12", Type.FromString("int")) }
        |> produces
            """
        
let x: int = 12

"""
