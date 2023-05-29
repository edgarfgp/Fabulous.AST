namespace Fabulous.AST.Tests.TypeDefinitions

open FSharp.Compiler.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Class =
    [<Test>]
    let ``Produces a class implicit constructor`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )
            
        AnonymousModule() {
            Class("Person", List.Empty) {
                EscapeHatch(memberNode)
            }
        }
        |> produces
            """
type Person =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )
            
        let param =
            SimplePatNode(None, false, SingleTextNode("name", Range.Zero), None, Range.Zero)

        AnonymousModule() {
            Class("Person", [ param ]) {
                EscapeHatch(memberNode)
            }
        }
        |> produces
            """
type Person (name) =
    member this.Name = name

"""