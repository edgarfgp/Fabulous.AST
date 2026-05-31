namespace Fabulous.AST.Tests.Core

open System
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Xunit

open type Ast

module GenParseTests =

    [<Fact>]
    let ``widget parses round-trip``() =
        let widget: WidgetBuilder<Oak> =
            Oak() {
                AnonymousModule() {
                    Value("greet", "x + 1")
                    TypeDefn("Greeter") { Member("this.Hello", "1") }
                }
            }

        Assert.Contains("greet", Gen.parse widget)

    [<Fact>]
    let ``composed mkOak |> run |> parse pipeline parses``() =
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("greet", "x + 1") } }

        Assert.Contains("greet", widget |> Gen.mkOak |> Gen.run |> Gen.parse)

    [<Fact>]
    let ``valid source string parses and round-trips``() =
        let source = "let x = 1\nlet y = x + 2\n"

        Assert.Equal(source, Gen.parse source)

    [<Fact>]
    let ``invalid source string returns located diagnostics, one per OS line``() =
        let source = "let x = \n  if then else"

        let result = Gen.parse source

        // Diagnostics carry a position and an FS error number...
        Assert.Contains("Error FS", result)
        // ...and the multiple diagnostics are separated by the OS newline.
        Assert.Contains(Environment.NewLine, result)

    [<Fact>]
    let ``widget producing invalid F# returns located diagnostics``() =
        // An incomplete expression renders to "let x = if then else", which is
        // syntactically broken — exactly the kind of codegen bug Gen.parse catches.
        let widget: WidgetBuilder<Oak> =
            Oak() { AnonymousModule() { Value("x", "if then else") } }

        Assert.Contains("Error FS", Gen.parse widget)
