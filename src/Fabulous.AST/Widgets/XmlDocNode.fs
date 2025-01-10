namespace Fabulous.AST

open System.Runtime.CompilerServices
open System
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module XmlDocNode =

    let Lines = Attributes.defineScalar<string list> "Lines"

    let Summary = Attributes.defineScalar<string list> "Lines"

    let Parameters = Attributes.defineScalar<(string * string) list> "Parameters"

    let ReturnInfo = Attributes.defineScalar<string list> "ReturnInfo"

    let ExceptionInfo = Attributes.defineScalar<(string * string) list> "ExceptionInfo"

    let WidgetKey =
        Widgets.register "XmlDocs" (fun widget ->
            let lines =
                Widgets.tryGetScalarValue widget Lines
                |> ValueOption.map(fun lines -> lines |> List.map(fun x -> $"/// {x}"))
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

            let rec processLines(lines: string list) =
                match lines with
                | [] -> []
                | lineA :: rest as lines ->
                    let lineAT = lineA.TrimStart([| ' ' |])

                    if String.IsNullOrEmpty(lineAT) then
                        processLines rest
                    elif lineAT.StartsWith("<", StringComparison.Ordinal) then
                        lines
                    else
                        [ "<summary>" ] @ (lines |> List.map escape) @ [ "</summary>" ]

            let composeSummary(unprocessedLines: string list) =
                processLines unprocessedLines |> List.map(fun x -> $"/// {x}")

            let composeParameters(parameters: (string * string) list) =
                parameters
                |> List.map(fun (name, desc) -> $"/// <param name=\"{name}\">{desc}</param>")

            let composeReturnInfo(returnInfo: string list) =
                returnInfo |> List.map(fun v -> $"/// <returns>{v}</returns>")

            let composeExceptionInfo(exceptionInfo: (string * string) list) =
                exceptionInfo
                |> List.map(fun (name, desc) -> $"/// <exception cref=\"{name}\">{desc}</exception>")

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
                |> List.concat
                |> Array.ofList

            XmlDocNode(lines, Range.Zero))

[<AutoOpen>]
module XmlDocsBuilders =
    type Ast with
        static member inline XmlDocs(lines: string list) =
            WidgetBuilder<XmlDocNode>(XmlDocNode.WidgetKey, XmlDocNode.Lines.WithValue(lines))

        static member inline XmlDocs(lines: string) = Ast.XmlDocs [ lines ]

        static member inline Summary(lines: string list) =
            WidgetBuilder<XmlDocNode>(XmlDocNode.WidgetKey, XmlDocNode.Summary.WithValue(lines))

        static member inline Summary(summary: string) = Ast.Summary [ summary ]

type XmlDocsModifiers =
    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, parameters: (string * string) list) =
        this.AddScalar(XmlDocNode.Parameters.WithValue(parameters))

    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, returnInfo: string * string) =
        XmlDocsModifiers.parameters(this, [ returnInfo ])

    [<Extension>]
    static member inline parameters(this: WidgetBuilder<XmlDocNode>, returnInfo: string, desc: string) =
        XmlDocsModifiers.parameters(this, [ returnInfo, desc ])

    [<Extension>]
    static member inline returnInfo(this: WidgetBuilder<XmlDocNode>, returnInfo: string list) =
        this.AddScalar(XmlDocNode.ReturnInfo.WithValue(returnInfo))

    [<Extension>]
    static member inline returnInfo(this: WidgetBuilder<XmlDocNode>, returnInfo: string) =
        XmlDocsModifiers.returnInfo(this, [ returnInfo ])

    [<Extension>]
    static member inline exceptionInfo(this: WidgetBuilder<XmlDocNode>, exceptionInfo: (string * string) list) =
        this.AddScalar(XmlDocNode.ExceptionInfo.WithValue(exceptionInfo))

    [<Extension>]
    static member inline exceptionInfo(this: WidgetBuilder<XmlDocNode>, exceptionInfo: string, desc: string) =
        XmlDocsModifiers.exceptionInfo(this, [ exceptionInfo, desc ])
