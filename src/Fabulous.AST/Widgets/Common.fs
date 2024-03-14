namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core.CompilerServices

type AccessControl =
    | Public
    | Private
    | Internal
    | Unknown

[<RequireQualifiedAccess>]
type StringOrWidget<'T> =
    | StringExpr of string
    | WidgetExpr of 'T

[<RequireQualifiedAccess>]
type ModuleOrNamespaceDecl =
    | Module of string array
    | Namespace of string
    | Anonymous

[<AutoOpen>]
module CommonExtensions =
    type MultipleTextsNode with

        static member Create(texts: string list) =
            MultipleTextsNode(
                [ for v in texts do
                      SingleTextNode.Create(v) ],
                Range.Zero
            )

    type XmlDocNode with

        static member Create(content: string list) =
            content
            |> List.map(fun v -> $"/// {v}")
            |> Array.ofList
            |> fun v -> XmlDocNode(v, Range.Zero)

    type TyparDecls with
        static member inline Postfix(decl: string) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero) ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Postfix(decls: string list) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ for v in decls do
                          TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Postfix(decl: TyparDeclNode) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(SingleTextNode.lessThan, [ decl ], [], SingleTextNode.greaterThan, Range.Zero)
            )

        static member inline Postfix(decls: TyparDeclNode list) =
            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    [ for v in decls do
                          v ],
                    [],
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            )

        static member inline Prefix(decl: string) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero) ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decls: string list) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ for v in decls do
                          TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decl: TyparDeclNode) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ decl ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline Prefix(decls: TyparDeclNode list) =
            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    [ for v in decls do
                          v ],
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            )

        static member inline SinglePrefix(decl: string) =
            TyparDecls.SinglePrefix(TyparDeclNode(None, SingleTextNode.Create decl, [], Range.Zero))

        static member inline SinglePrefix(decl: TyparDeclNode) = TyparDecls.SinglePrefix(decl)

[<RequireQualifiedAccess>]
module List =
    let intersperse separator (source: List<'T>) =
        let mutable coll = new ListCollector<'T>()

        let mutable notFirst = false

        source
        |> List.iter(fun element ->
            if notFirst then
                coll.Add separator

            coll.Add element
            notFirst <- true)

        coll.Close()

[<RequireQualifiedAccess>]
module StringParsing =
    /// Adds double backticks to the identifier if necessary.
    let normalizeIdentifierBackticks(identifier: string) =
        if System.String.IsNullOrEmpty identifier then
            failwith "This is not a valid identifier"
        else
            let trimmed = identifier.Trim()
            Fantomas.FCS.Syntax.PrettyNaming.NormalizeIdentifierBackticks trimmed

    /// Adds quotes to the identifier if necessary.
    let normalizeIdentifierQuotes(identifier: string, hasQuotes) =
        if identifier = null then
            failwith "This is not a valid identifier"
        else if hasQuotes then
            $"\"{identifier}\""
        else
            identifier

    let normalizeModuleOrNamespaceName(name: string) =
        if System.String.IsNullOrEmpty name then
            ModuleOrNamespaceDecl.Anonymous
        else if name.Contains(".") then
            let trimmed = name.Trim()
            let split = trimmed.Split('.')
            split |> Array.map normalizeIdentifierBackticks |> ModuleOrNamespaceDecl.Module
        else
            ModuleOrNamespaceDecl.Namespace name
