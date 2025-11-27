namespace Fabulous.AST

open System.Runtime.CompilerServices
open System
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module XmlDocNode =

    let Lines = Attributes.defineScalar<string seq> "Lines"

    let Summary = Attributes.defineScalar<string seq> "Lines"

    let Parameters = Attributes.defineScalar<(string * string) seq> "Parameters"

    let ReturnInfo = Attributes.defineScalar<string seq> "ReturnInfo"

    let ExceptionInfo = Attributes.defineScalar<(string * string) seq> "ExceptionInfo"

    let WidgetKey =
        Widgets.register "XmlDocs" (fun widget ->
            let lines =
                Widgets.tryGetScalarValue widget Lines
                |> ValueOption.map(fun lines -> lines |> Seq.map(fun x -> $"/// {x}"))
                |> ValueOption.defaultValue []

            let getEscapeSequence c =
                match c with
                | '<' -> "&lt;"
                | '>' -> "&gt;"
                | '\"' -> "&quot;"
                | '\'' -> "&apos;"
                | '&' -> "&amp;"
                | ch -> ch.ToString()

            let escape str = String.collect getEscapeSequence str

            let rec processLines(lines: string seq) =
                match List.ofSeq lines with
                | [] -> []
                | lineA :: rest as lines ->
                    let lineAT = lineA.TrimStart([| ' ' |])

                    if String.IsNullOrEmpty(lineAT) then
                        processLines rest
                    elif lineAT.StartsWith("<", StringComparison.Ordinal) then
                        lines
                    else
                        [ "<summary>" ] @ (lines |> List.map escape) @ [ "</summary>" ]

            let composeSummary(unprocessedLines: string seq) =
                processLines unprocessedLines |> List.map(fun x -> $"/// {x}")

            let composeParameters(parameters: (string * string) seq) =
                parameters
                |> Seq.map(fun (name, desc) -> $"/// <param name=\"{name}\">{desc}</param>")

            let composeReturnInfo(returnInfo: string seq) =
                returnInfo |> Seq.map(fun v -> $"/// <returns>{v}</returns>")

            let composeExceptionInfo(exceptionInfo: (string * string) seq) =
                exceptionInfo
                |> Seq.map(fun (name, desc) -> $"/// <exception cref=\"{name}\">{desc}</exception>")

            let summary =
                Widgets.tryGetScalarValue widget Summary
                |> ValueOption.map composeSummary
                |> ValueOption.defaultValue []

            let parameters =
                Widgets.tryGetScalarValue widget Parameters
                |> ValueOption.map composeParameters
                |> ValueOption.defaultValue []

            let returnInfo =
                Widgets.tryGetScalarValue widget ReturnInfo
                |> ValueOption.map composeReturnInfo
                |> ValueOption.defaultValue []

            let exceptionInfo =
                Widgets.tryGetScalarValue widget ExceptionInfo
                |> ValueOption.map composeExceptionInfo
                |> ValueOption.defaultValue []

            let lines =
                [ lines; summary; parameters; returnInfo; exceptionInfo ]
                |> Seq.concat
                |> Array.ofSeq

            XmlDocNode(lines, Range.Zero))

[<AutoOpen>]
module XmlDocsBuilders =
    type Ast with
        /// <summary>Creates XML documentation with the specified lines.</summary>
        /// <param name="lines">The seq of documentation lines.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///             .xmlDocs(XmlDocs(["This is a value"; "It represents an integer"]))
        ///     }
        /// }
        /// </code>
        static member inline XmlDocs(lines: string seq) =
            WidgetBuilder<XmlDocNode>(XmlDocNode.WidgetKey, XmlDocNode.Lines.WithValue(lines))

        /// <summary>Creates XML documentation with a single line.</summary>
        /// <param name="lines">The documentation text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///             .xmlDocs(XmlDocs("This is a value"))
        ///     }
        /// }
        /// </code>
        static member inline XmlDocs(lines: string) = Ast.XmlDocs [ lines ]

        /// <summary>Creates a summary XML documentation with the specified lines.</summary>
        /// <param name="lines">The seq of summary lines.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///             .xmlDocs(Summary(["This is a value"; "It represents an integer"]))
        ///     }
        /// }
        /// </code>
        static member inline Summary(lines: string seq) =
            WidgetBuilder<XmlDocNode>(XmlDocNode.WidgetKey, XmlDocNode.Summary.WithValue(lines))

        /// <summary>Creates a summary XML documentation with a single line.</summary>
        /// <param name="summary">The summary text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///             .xmlDocs(Summary("This is a value"))
        ///     }
        /// }
        /// </code>
        static member inline Summary(summary: string) = Ast.Summary [ summary ]

type XmlDocsModifiers =
    /// <summary>Adds parameter documentation to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="parameters">List of parameter name and description pairs.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("add", "int -> int -> int")
    ///             .xmlDocs(
    ///                 Summary("Adds two integers")
    ///                     .parameters([("x", "First integer"); ("y", "Second integer")])
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, parameters: (string * string) seq) =
        this.AddScalar(XmlDocNode.Parameters.WithValue(parameters))

    /// <summary>Adds a single parameter documentation to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="returnInfo">Parameter name and description pair.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("getValue", "unit -> int")
    ///             .xmlDocs(
    ///                 Summary("Gets a value")
    ///                     .parameters(("unit", "No parameters"))
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, returnInfo: string * string) =
        XmlDocsModifiers.parameters(this, [ returnInfo ])

    /// <summary>Adds a single parameter documentation to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="returnInfo">Parameter name.</param>
    /// <param name="desc">Parameter description.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("getValue", "unit -> int")
    ///             .xmlDocs(
    ///                 Summary("Gets a value")
    ///                     .parameters("unit", "No parameters")
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, returnInfo: string, desc: string) =
        XmlDocsModifiers.parameters(this, [ returnInfo, desc ])

    /// <summary>Adds return value documentation to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="returnInfo">List of return value description lines.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("getValue", "unit -> int")
    ///             .xmlDocs(
    ///                 Summary("Gets a value")
    ///                     .returnInfo(["The integer value"; "Can be positive or negative"])
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline returnInfo(this: WidgetBuilder<XmlDocNode>, returnInfo: string seq) =
        this.AddScalar(XmlDocNode.ReturnInfo.WithValue(returnInfo))

    /// <summary>Adds return value documentation to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="returnInfo">Return value description.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("getValue", "unit -> int")
    ///             .xmlDocs(
    ///                 Summary("Gets a value")
    ///                     .returnInfo("The integer value")
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline returnInfo(this: WidgetBuilder<XmlDocNode>, returnInfo: string) =
        XmlDocsModifiers.returnInfo(this, [ returnInfo ])

    /// <summary>Adds exception information to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="exceptionInfo">List of exception type and description pairs.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("divide", "int -> int -> int")
    ///             .xmlDocs(
    ///                 Summary("Divides two integers")
    ///                     .parameters([("x", "Numerator"); ("y", "Denominator")])
    ///                     .exceptionInfo([("System.DivideByZeroException", "Thrown when denominator is zero")])
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline exceptionInfo(this: WidgetBuilder<XmlDocNode>, exceptionInfo: (string * string) seq) =
        this.AddScalar(XmlDocNode.ExceptionInfo.WithValue(exceptionInfo))

    /// <summary>Adds exception information to XML documentation.</summary>
    /// <param name="this">Current XML documentation widget.</param>
    /// <param name="exceptionInfo">Exception type.</param>
    /// <param name="desc">Exception description.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("divide", "int -> int -> int")
    ///             .xmlDocs(
    ///                 Summary("Divides two integers")
    ///                     .parameters([("x", "Numerator"); ("y", "Denominator")])
    ///                     .exceptionInfo("System.DivideByZeroException", "Thrown when denominator is zero")
    ///             )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline exceptionInfo(this: WidgetBuilder<XmlDocNode>, exceptionInfo: string, desc: string) =
        XmlDocsModifiers.exceptionInfo(this, [ exceptionInfo, desc ])
