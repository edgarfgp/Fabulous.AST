namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module Namespace =
    [<Fact>]
    let ``Produces a namespace with binding``() =
        (Namespace("Fabulous.AST") { Value("x", "3").hasQuotes(false) })
        |> produces
            """
namespace Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a rec namespace with binding``() =
        (Namespace("Fabulous.AST") { Value("x", "3").hasQuotes(false) }).toRecursive()
        |> produces
            """
namespace rec Fabulous.AST

let x = 3
"""

    [<Fact>]
    let ``Produces a namespace using the EscapeHatch widget``() =
        Namespace("Fabulous.AST") {
            BindingNode(
                None,
                None,
                MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                false,
                None,
                None,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                None,
                List.Empty,
                None,
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                Range.Zero
            )
        }
        |> produces
            """
namespace Fabulous.AST

let x = 12
"""

    [<Fact>]
    let ``Produces a namespace with nested module``() =
        Namespace("Fabulous") {
            NestedModule("AST") {
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                    Range.Zero
                )
            }
        }
        |> produces
            """
namespace Fabulous

module AST =
    let x = 12
"""

    type Person =
        { typename: string
          props: Map<string, string list> }

    [<Fact>]
    let ``Produces a namespace with nested module using yield bang``() =
        let records =
            [ { typename = "Person"
                props = [ ("Age", [ "int" ]); ("Name", [ "string" ]) ] |> Map.ofList } ]

        let recordTypes =
            records
            |> List.map
                (fun
                    { typename = name
                      props = props } ->
                    if Map.isEmpty props then
                        Abbrev(name, Obj()) |> Gen.mkOak |> TypeDefn.Abbrev |> ModuleDecl.TypeDefn
                    else
                        let rec mkType(value: string list) =
                            match value with
                            | [] -> failwith "unexpected"
                            | [ single ] -> LongIdent(single)
                            | head :: tail -> AppPostfix(mkType(tail), LongIdent(head))

                        let myFields =
                            props |> Map.toList |> List.map(fun (key, value) -> Field(key, mkType value))

                        let fields = Record(name) { yield! myFields } |> Gen.mkOak
                        let record = TypeDefn.Record(fields)
                        ModuleDecl.TypeDefn(record))

        Namespace("Json") {
            for recordType in recordTypes do
                EscapeHatch(recordType)
        }
        |> produces
            """
namespace Json

type Person = { Age: int; Name: string }
"""
