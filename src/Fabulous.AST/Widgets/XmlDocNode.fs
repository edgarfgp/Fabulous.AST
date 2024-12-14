namespace Fabulous.AST

open System.Runtime.CompilerServices
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
            let lines = Widgets.tryGetScalarValue widget Lines |> ValueOption.defaultValue []

            let lines = lines |> List.map(fun x -> $"/// {x}") |> Array.ofList

            let summary =
                Widgets.tryGetScalarValue widget Summary |> ValueOption.defaultValue []

            let summary =
                summary
                |> List.mapi(fun i v ->
                    if i = 0 then $"/// <summary>{v}"
                    elif i = summary.Length - 1 then $"/// {v}</summary>"
                    else $"/// {v}")
                |> Array.ofList

            let parameters =
                Widgets.tryGetScalarValue widget Parameters |> ValueOption.defaultValue []

            let parameters =
                parameters
                |> List.map(fun (name, desc) -> $"/// <param name=\"{name}\">{desc}</param>")
                |> Array.ofList

            let returnInfo =
                Widgets.tryGetScalarValue widget ReturnInfo |> ValueOption.defaultValue []

            let returnInfo =
                returnInfo |> List.map(fun v -> $"/// <returns>{v}</returns>") |> Array.ofList

            let exceptionInfo =
                Widgets.tryGetScalarValue widget ExceptionInfo |> ValueOption.defaultValue []

            let exceptionInfo =
                exceptionInfo
                |> List.map(fun (name, desc) -> $"/// <exception cref=\"{name}\">{desc}</exception>")
                |> Array.ofList

            let lines = Array.concat [ lines; summary; parameters; returnInfo; exceptionInfo ]
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
