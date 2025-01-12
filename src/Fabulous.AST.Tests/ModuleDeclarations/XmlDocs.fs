namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module XmlDocs =
    [<Fact>]
    let ``Produces XmlDocs with no tags``() =
        Oak() {
            AnonymousModule() {
                Val("collect", Funs([ Char(); String(); String() ], String()))
                    .xmlDocs(
                        [ "Creates a new string whose characters are the result of applying"
                          "the function mapping to each of the characters of the input string"
                          "and concatenating the resulting strings." ]
                    )
            }
        }
        |> produces
            """
/// Creates a new string whose characters are the result of applying
/// the function mapping to each of the characters of the input string
/// and concatenating the resulting strings.
val collect: char -> string -> string -> string
"""

    [<Fact>]
    let ``Produces XmlDocs with summary tags``() =
        Oak() {
            AnonymousModule() {
                Val("collect", Funs([ Char(); String(); String() ], String()))
                    .xmlDocs(
                        Summary(
                            [ "Creates a new string whose characters are the result of applying"
                              "the function mapping to each of the characters of the input string"
                              "and concatenating the resulting strings." ]
                        )
                    )
            }
        }
        |> produces
            """
/// <summary>
/// Creates a new string whose characters are the result of applying
/// the function mapping to each of the characters of the input string
/// and concatenating the resulting strings.
/// </summary>
val collect: char -> string -> string -> string
"""

    [<Fact>]
    let ``Produces XmlDocs with summary and parameter tags``() =
        Oak() {
            AnonymousModule() {
                Val("collect", Funs([ Char(); String(); String() ], String()))
                    .xmlDocs(
                        Summary(
                            [ "Builds a new string whose characters are the results of applying the function <c>mapping</c>"
                              "to each of the characters of the input string and concatenating the resulting"
                              "strings." ]
                        )
                            .parameters(
                                [ ("mapping",
                                   "The function to produce a string from each character of the input string.")
                                  ("str", "The input string.") ]
                            )
                    )
            }
        }
        |> produces
            """
/// <summary>
/// Builds a new string whose characters are the results of applying the function &lt;c&gt;mapping&lt;/c&gt;
/// to each of the characters of the input string and concatenating the resulting
/// strings.
/// </summary>
/// <param name="mapping">The function to produce a string from each character of the input string.</param>
/// <param name="str">The input string.</param>
val collect: char -> string -> string -> string
"""

    [<Fact>]
    let ``Produces XmlDocs with summary parameters and returns tags``() =
        Oak() {
            AnonymousModule() {
                Val("collect", Funs([ Char(); String(); String() ], String()))
                    .xmlDocs(
                        Summary(
                            [ "Builds a new string whose characters are the results of applying the function <c>mapping</c>"
                              "to each of the characters of the input string and concatenating the resulting"
                              "strings." ]
                        )
                            .parameters(
                                [ ("mapping",
                                   "The function to produce a string from each character of the input string.")
                                  ("str", "The input string.") ]
                            )
                            .returnInfo("The concatenated string.")
                    )
            }
        }
        |> produces
            """
/// <summary>
/// Builds a new string whose characters are the results of applying the function &lt;c&gt;mapping&lt;/c&gt;
/// to each of the characters of the input string and concatenating the resulting
/// strings.
/// </summary>
/// <param name="mapping">The function to produce a string from each character of the input string.</param>
/// <param name="str">The input string.</param>
/// <returns>The concatenated string.</returns>
val collect: char -> string -> string -> string
"""

    [<Fact>]
    let ``Produces XmlDocs with summary parameters returns and exception tags``() =
        Oak() {
            AnonymousModule() {
                Val("collect", Funs([ Char(); String(); String() ], String()))
                    .xmlDocs(
                        Summary(
                            [ "Builds a new string whose characters are the results of applying the function <c>mapping</c>"
                              "to each of the characters of the input string and concatenating the resulting"
                              "strings." ]
                        )
                            .parameters(
                                [ ("mapping",
                                   "The function to produce a string from each character of the input string.")
                                  ("str", "The input string.") ]
                            )

                            .returnInfo("The concatenated string.")
                            .exceptionInfo(
                                [ ("ArgumentNullException", "Thrown when <paramref name=\"mapping\"/> is null.") ]
                            )
                    )
            }
        }
        |> produces
            """
/// <summary>
/// Builds a new string whose characters are the results of applying the function &lt;c&gt;mapping&lt;/c&gt;
/// to each of the characters of the input string and concatenating the resulting
/// strings.
/// </summary>
/// <param name="mapping">The function to produce a string from each character of the input string.</param>
/// <param name="str">The input string.</param>
/// <returns>The concatenated string.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping"/> is null.</exception>
val collect: char -> string -> string -> string
"""
