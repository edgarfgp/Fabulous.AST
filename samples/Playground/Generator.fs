namespace Playground

open Fabulous.AST
open type Fabulous.AST.Ast
open Fantomas.Core
open Common

module Generator =
    let source outputPath =
        async {
            let! sourceText =
                Oak() {
                    Namespace("MyNamespace") {
                        Module("MyModule") {
                            Value("y", "12")
                            Value("y", "13")
                        }
                    }
                }
                |> Gen.mkOak
                |> CodeFormatter.FormatOakAsync

            do! writeIfChanged outputPath sourceText
        }
