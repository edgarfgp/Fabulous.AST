namespace Fabulous.AST.Tests

open Fabulous.AST

open Xunit

open type Ast

module XmlDocs =
    [<Fact>]
    let ``Produces an XmlDocs with Summary single line``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
                    .xmlDocs(Summary("Calculate the volume of a cylinder."))
            }
        }
        |> produces
            """
/// <summary>
/// Calculate the volume of a cylinder.
/// </summary>
let cylinderVolume radius length =
    let pi = 3.14159
    length * pi * radius * radius
"""

    [<Fact>]
    let ``Produces an XmlDocs with Summary multiple lines``() =
        let xmlDocs =
            Summary(
                [ "Calculate the volume of a cylinder."
                  "It is calculated by multiplying the length by the area of the base."
                  "The area of the base is calculated by multiplying pi by the radius squared." ]
            )

        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
                    .xmlDocs(xmlDocs)
            }
        }
        |> produces
            """
/// <summary>
/// Calculate the volume of a cylinder.
/// It is calculated by multiplying the length by the area of the base.
/// The area of the base is calculated by multiplying pi by the radius squared.
/// </summary>
let cylinderVolume radius length =
    let pi = 3.14159
    length * pi * radius * radius
"""

    [<Fact>]
    let ``Produces an XmlDocs with Summary multiple lines and parameters``() =
        let xmlDocs =
            Summary(
                [ "Calculate the volume of a cylinder."
                  "It is calculated by multiplying the length by the area of the base."
                  "The area of the base is calculated by multiplying pi by the radius squared." ]
            )
                .parameters(
                    [ ("radius", "The radius of the cylinder.")
                      ("length", "The length of the cylinder.") ]
                )

        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
                    .xmlDocs(xmlDocs)
            }
        }
        |> produces
            """
/// <summary>
/// Calculate the volume of a cylinder.
/// It is calculated by multiplying the length by the area of the base.
/// The area of the base is calculated by multiplying pi by the radius squared.
/// </summary>
/// <param name="radius">The radius of the cylinder.</param>
/// <param name="length">The length of the cylinder.</param>
let cylinderVolume radius length =
    let pi = 3.14159
    length * pi * radius * radius
"""

    [<Fact>]
    let ``Produces an XmlDocs with Summary multiple lines, parameters and return info``() =
        let xmlDocs =
            Summary(
                [ "Calculate the volume of a cylinder."
                  "It is calculated by multiplying the length by the area of the base."
                  "The area of the base is calculated by multiplying pi by the radius squared." ]
            )
                .parameters(
                    [ ("radius", "The radius of the cylinder.")
                      ("length", "The length of the cylinder.") ]
                )
                .returnInfo("The volume of the cylinder.")

        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
                    .xmlDocs(xmlDocs)
            }
        }
        |> produces
            """
/// <summary>
/// Calculate the volume of a cylinder.
/// It is calculated by multiplying the length by the area of the base.
/// The area of the base is calculated by multiplying pi by the radius squared.
/// </summary>
/// <param name="radius">The radius of the cylinder.</param>
/// <param name="length">The length of the cylinder.</param>
/// <returns>The volume of the cylinder.</returns>
let cylinderVolume radius length =
    let pi = 3.14159
    length * pi * radius * radius
"""

    [<Fact>]
    let ``Produces an XmlDocs with Summary multiple lines, parameters, return info and exception info``() =
        let xmlDocs =
            Summary(
                [ "Calculate the volume of a cylinder."
                  "It is calculated by multiplying the length by the area of the base."
                  "The area of the base is calculated by multiplying pi by the radius squared." ]
            )
                .parameters(
                    [ ("radius", "The radius of the cylinder.")
                      ("length", "The length of the cylinder.") ]
                )
                .returnInfo("The volume of the cylinder.")
                .exceptionInfo([ ("System.Exception", "If the radius or length is less than zero.") ])

        Oak() {
            AnonymousModule() {
                Function(
                    "cylinderVolume",
                    [ ParameterPat "radius"; ParameterPat "length" ],
                    [ LetOrUseExpr(Value("pi", Double(3.14159)))
                      OtherExpr(
                          InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
                      ) ]
                )
                    .xmlDocs(xmlDocs)
            }
        }
        |> produces
            """
/// <summary>
/// Calculate the volume of a cylinder.
/// It is calculated by multiplying the length by the area of the base.
/// The area of the base is calculated by multiplying pi by the radius squared.
/// </summary>
/// <param name="radius">The radius of the cylinder.</param>
/// <param name="length">The length of the cylinder.</param>
/// <returns>The volume of the cylinder.</returns>
/// <exception cref="System.Exception">If the radius or length is less than zero.</exception>
let cylinderVolume radius length =
    let pi = 3.14159
    length * pi * radius * radius
"""
