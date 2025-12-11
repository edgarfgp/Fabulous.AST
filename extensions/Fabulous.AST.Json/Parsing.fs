namespace Fabulous.AST.Json

open System
open System.Globalization
open System.Text.Json
open System.Text.Json.Nodes
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parsing =

    // ─────────────────────────────────────────────────────────────────────────
    // Naming Utilities
    // ─────────────────────────────────────────────────────────────────────────

    let toPascalCase (s: string) (fallback: string) =
        let s = if String.IsNullOrWhiteSpace(s) then fallback else s

        let parts =
            s
            |> Seq.map(fun c -> if Char.IsLetterOrDigit(c) then c else ' ')
            |> Seq.toArray
            |> String
            |> fun t -> t.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)

        if parts.Length = 0 then
            fallback
        else
            let result =
                parts
                |> Array.map(fun p ->
                    if p.Length = 0 then
                        p
                    else
                        let first = Char.ToUpper(p[0], CultureInfo.InvariantCulture)

                        if p.Length = 1 then
                            string first
                        else
                            string first + p.Substring(1))
                |> String.concat ""

            // F# identifiers cannot start with a digit, prefix with underscore if needed
            if result.Length > 0 && Char.IsDigit(result[0]) then
                "_" + result
            else
                result

    /// F# reserved keywords that cannot be used as identifiers
    let private fsharpKeywords =
        Set.ofList
            [ "abstract"
              "and"
              "as"
              "assert"
              "base"
              "begin"
              "class"
              "default"
              "delegate"
              "do"
              "done"
              "downcast"
              "downto"
              "elif"
              "else"
              "end"
              "exception"
              "extern"
              "false"
              "finally"
              "fixed"
              "for"
              "fun"
              "function"
              "global"
              "if"
              "in"
              "inherit"
              "inline"
              "interface"
              "internal"
              "lazy"
              "let"
              "match"
              "member"
              "module"
              "mutable"
              "namespace"
              "new"
              "not"
              "null"
              "of"
              "open"
              "or"
              "override"
              "private"
              "public"
              "rec"
              "return"
              "select"
              "static"
              "struct"
              "then"
              "to"
              "true"
              "try"
              "type"
              "upcast"
              "use"
              "val"
              "void"
              "when"
              "while"
              "with"
              "yield"
              // OCaml keywords for compatibility
              "asr"
              "land"
              "lor"
              "lsl"
              "lsr"
              "lxor"
              "mod"
              "sig" ]

    /// Check if a string contains characters that require backtick escaping in F#
    let private requiresBacktickEscaping(name: string) : bool =
        name
        |> Seq.exists(fun c -> not(Char.IsLetterOrDigit(c)) && c <> '_' && c <> '\'')

    /// Sanitize a field name for F# (handles leading digits, reserved keywords, and special characters)
    let sanitizeFieldName(name: string) : string =
        if String.IsNullOrEmpty(name) then
            "_field"
        elif Char.IsDigit(name[0]) then
            "_" + name
        elif fsharpKeywords.Contains(name) || requiresBacktickEscaping name then
            // Escape any existing backticks by doubling them
            let escaped = name.Replace("``", "````")
            "``" + escaped + "``"
        else
            name

    // ─────────────────────────────────────────────────────────────────────────
    // AST Helpers
    // ─────────────────────────────────────────────────────────────────────────

    let ident(name: string) = SingleTextNode(name, Range.Zero)

    let identList(name: string) =
        IdentListNode([ IdentifierOrDot.Ident(ident name) ], Range.Zero)

    let longIdent(name: string) = Type.LongIdent(identList name)

    let listType(t: Type) : Type =
        Type.AppPostfix(TypeAppPostFixNode(t, longIdent "list", Range.Zero))

    let optionType(t: Type) : Type =
        Type.AppPostfix(TypeAppPostFixNode(t, longIdent "option", Range.Zero))

    // ─────────────────────────────────────────────────────────────────────────
    // Type Inference Helpers
    // ─────────────────────────────────────────────────────────────────────────

    let tryJsonValue<'T>(v: JsonValue) : bool =
        try
            let _ = v.GetValue<'T>()
            true
        with
        | :? InvalidOperationException
        | :? FormatException -> false

    /// Get the JsonValueKind for a JsonValue without throwing.
    /// Prefer JsonValue.GetValueKind() (STJ 8+) and fall back to obtaining a JsonElement.
    let getKind(v: JsonValue) : JsonValueKind =
        try
            // Available on System.Text.Json 8+
            v.GetValueKind()
        with _ ->
            try
                v.GetValue<JsonElement>().ValueKind
            with _ ->
                JsonValueKind.Undefined

    /// Infer type from a JsonValue primitive
    let inferPrimitiveType(v: JsonValue) : Type =
        match getKind v with
        | JsonValueKind.True
        | JsonValueKind.False -> longIdent "bool"
        | JsonValueKind.String -> longIdent "string"
        | JsonValueKind.Number ->
            // Check int first, then int64 for large integers, then float for decimals
            // Note: We use float (not decimal) as the default for floating-point numbers
            // because it's more common in JSON APIs and F# serialization libraries
            if tryJsonValue<int> v then longIdent "int"
            elif tryJsonValue<int64> v then longIdent "int64"
            elif tryJsonValue<double> v then longIdent "float"
            else longIdent "float"
        | JsonValueKind.Null
        | JsonValueKind.Undefined
        | _ -> longIdent "obj"

    // ─────────────────────────────────────────────────────────────────────────
    // Generation State
    // ─────────────────────────────────────────────────────────────────────────

    type GenState =
        { Emitted: Set<string>
          DeclsRev: ModuleDecl list }

    let emptyState =
        { Emitted = Set.empty
          DeclsRev = [] }

    let addDecl (decl: ModuleDecl) (st: GenState) =
        { st with DeclsRev = decl :: st.DeclsRev }

    let markEmitted (name: string) (st: GenState) =
        { st with Emitted = st.Emitted.Add name }

    // ─────────────────────────────────────────────────────────────────────────
    // AST Node Builders
    // ─────────────────────────────────────────────────────────────────────────

    let mkTypeNameNode(typeName: string) =
        TypeNameNode(
            None,
            None,
            SingleTextNode.``type``,
            None,
            identList typeName,
            None,
            [],
            None,
            Some(SingleTextNode.equals),
            None,
            Range.Zero
        )

    let mkRecordDecl (typeName: string) (fields: FieldNode list) : ModuleDecl =
        let recordNode =
            TypeDefnRecordNode(
                mkTypeNameNode typeName,
                None,
                SingleTextNode.leftCurlyBrace,
                fields,
                SingleTextNode.rightCurlyBrace,
                [],
                Range.Zero
            )

        ModuleDecl.TypeDefn(TypeDefn.Record(recordNode))

    let mkAliasDecl (name: string) (t: Type) : ModuleDecl =
        TypeDefnAbbrevNode(mkTypeNameNode name, t, [], Range.Zero)
        |> TypeDefn.Abbrev
        |> ModuleDecl.TypeDefn

    // ─────────────────────────────────────────────────────────────────────────
    // Array-of-Objects Analysis (Extracted for Reuse)
    // ─────────────────────────────────────────────────────────────────────────

    /// Result of analyzing an array of JSON objects
    type ObjectArrayAnalysis =
        { Keys: string list
          OptionalKeys: Set<string>
          TypeOverrides: Map<string, Type> }

    /// Extract all JsonObjects from a JsonArray
    let extractObjects(arr: JsonArray) : JsonObject array =
        arr
        |> Seq.choose (function
            | :? JsonObject as o -> Some o
            | _ -> None)
        |> Seq.toArray

    /// Collect union of keys from objects, preserving first-seen order
    let collectKeys(objs: JsonObject array) : string list =
        let keysRev, _ =
            (([], Set.empty<string>), objs)
            ||> Array.fold(fun (keysRev, seen) o ->
                ((keysRev, seen), Seq.toArray o)
                ||> Array.fold(fun (keysRev, seen) p ->
                    if seen.Contains p.Key then
                        keysRev, seen
                    else
                        p.Key :: keysRev, seen.Add p.Key))

        List.rev keysRev

    /// Analyze an array of objects to determine optional keys and infer types.
    /// The genTypeFn parameter allows recursive type generation.
    let analyzeObjectArray
        (objs: JsonObject array)
        (genTypeFn: string -> JsonNode -> GenState -> Type * GenState)
        (st: GenState)
        : ObjectArrayAnalysis * GenState =

        let keys = collectKeys objs

        let optionKeys, typeOverrides, st' =
            ((Set.empty<string>, Map.empty<string, Type>, st), keys)
            ||> List.fold(fun (optSet, overrides, stAcc) key ->
                let values =
                    objs |> Array.choose(fun o -> if o.ContainsKey key then Some o[key] else None)

                let hasMissing = values.Length <> objs.Length
                let hasNull = values |> Array.exists isNull
                let optSet' = if hasMissing || hasNull then Set.add key optSet else optSet

                let sampleOpt = values |> Array.tryFind(fun v -> not(isNull v))

                let fieldType, stAcc' =
                    match sampleOpt with
                    | Some v -> genTypeFn (toPascalCase key "Field") v stAcc
                    | None -> longIdent "obj", stAcc

                optSet', Map.add key fieldType overrides, stAcc')

        let analysis =
            { Keys = keys
              OptionalKeys = optionKeys
              TypeOverrides = typeOverrides }

        analysis, st'

    /// Build a representative JsonObject containing all keys (for ensureRecordWith)
    let buildRepresentativeObject(keys: string list) : JsonObject =
        let rep = JsonObject()
        keys |> List.iter(fun k -> rep.Add(k, JsonValue.Create ""))
        rep

    // ─────────────────────────────────────────────────────────────────────────
    // Main Generation Logic
    // ─────────────────────────────────────────────────────────────────────────

    /// Generate a ModuleOrNamespaceNode containing type declarations inferred from the given JSON input.
    let generateModule
        (rootNameOpt: voption<string>)
        (nodeOptions: JsonNodeOptions)
        (documentOptions: JsonDocumentOptions)
        (input: string)
        : ModuleOrNamespaceNode =

        let root =
            try
                JsonNode.Parse(input, nodeOptions, documentOptions)
            with ex ->
                // Avoid logging raw JSON content
                let rootHint =
                    match rootNameOpt with
                    | ValueSome rn when not(String.IsNullOrWhiteSpace rn) -> rn.Trim()
                    | _ -> "Root"

                raise(
                    JsonException(
                        $"Failed to parse JSON input in generateModule (root '{rootHint}'). Consider enabling comments or trailing commas in options. Effective options: AllowTrailingCommas={documentOptions.AllowTrailingCommas}; CommentHandling={documentOptions.CommentHandling}; MaxDepth={documentOptions.MaxDepth}",
                        ex
                    )
                )

        let resolvedRootName =
            match rootNameOpt with
            | ValueSome rn when not(String.IsNullOrWhiteSpace rn) -> toPascalCase rn "Root"
            | _ -> "Root"

        let resolvedItemName = toPascalCase $"{resolvedRootName}Item" "Item"

        // ─────────────────────────────────────────────────────────────────
        // Recursive type generation
        // ─────────────────────────────────────────────────────────────────

        let rec genType (suggestedName: string) (node: JsonNode) (st: GenState) : Type * GenState =
            match node with
            | :? JsonObject as obj ->
                let typeName = toPascalCase suggestedName "Anon"
                let st' = ensureRecord typeName obj st
                longIdent typeName, st'

            | :? JsonArray as arr -> genArrayType suggestedName arr st

            | :? JsonValue as v -> inferPrimitiveType v, st

            | _ -> longIdent "obj", st

        and genArrayType (suggestedName: string) (arr: JsonArray) (st: GenState) : Type * GenState =
            if arr.Count = 0 then
                listType(longIdent "obj"), st
            else
                let elemName = toPascalCase (suggestedName + "Item") "Item"

                match arr[0] with
                | :? JsonObject ->
                    let objs = extractObjects arr

                    if objs.Length = 0 then
                        let elemType, st' = genType elemName arr[0] st
                        listType elemType, st'
                    else
                        let analysis, st' = analyzeObjectArray objs genType st
                        let rep = buildRepresentativeObject analysis.Keys

                        let st'' =
                            ensureRecordWith elemName rep analysis.OptionalKeys analysis.TypeOverrides st'

                        listType(longIdent elemName), st''

                | first ->
                    let elemType, st' = genType elemName first st
                    listType elemType, st'

        and ensureRecordWith
            (typeName: string)
            (obj: JsonObject)
            (optionKeys: Set<string>)
            (typeOverrides: Map<string, Type>)
            (st: GenState)
            : GenState =

            if st.Emitted.Contains typeName then
                st
            else
                let fieldsRev, st' =
                    (([], st), Seq.toArray obj)
                    ||> Array.fold(fun (accFields, stAcc) prop ->
                        let fname = prop.Key
                        let fnode = prop.Value

                        let baseType, stAcc' =
                            match Map.tryFind fname typeOverrides with
                            | Some t -> t, stAcc
                            | None ->
                                if isNull fnode then
                                    longIdent "obj", stAcc
                                else
                                    genType (toPascalCase fname "Field") fnode stAcc

                        let ftype =
                            if optionKeys.Contains fname then
                                optionType baseType
                            else
                                baseType

                        // Sanitize field name for F# (handles leading digits)
                        let sanitizedFieldName = sanitizeFieldName fname

                        let fieldNode =
                            FieldNode(None, None, None, None, None, Some(ident sanitizedFieldName), ftype, Range.Zero)

                        fieldNode :: accFields, stAcc')

                let fields = List.rev fieldsRev
                let decl = mkRecordDecl typeName fields
                st' |> addDecl decl |> markEmitted typeName

        and ensureRecord (typeName: string) (obj: JsonObject) (st: GenState) : GenState =
            ensureRecordWith typeName obj Set.empty Map.empty st

        // ─────────────────────────────────────────────────────────────────
        // Top-level generation based on root node type
        // ─────────────────────────────────────────────────────────────────

        let generateRootArray(arr: JsonArray) : GenState =
            if arr.Count = 0 then
                emptyState |> addDecl(mkAliasDecl resolvedRootName (listType(longIdent "obj")))
            else
                match arr[0] with
                | :? JsonObject ->
                    let objs = extractObjects arr

                    if objs.Length = 0 then
                        let elemType, st' = genType resolvedItemName arr[0] emptyState
                        st' |> addDecl(mkAliasDecl resolvedRootName (listType elemType))
                    else
                        let analysis, st' = analyzeObjectArray objs genType emptyState
                        let rep = buildRepresentativeObject analysis.Keys

                        let st'' =
                            ensureRecordWith resolvedItemName rep analysis.OptionalKeys analysis.TypeOverrides st'

                        st''
                        |> addDecl(mkAliasDecl resolvedRootName (listType(longIdent resolvedItemName)))

                | elem ->
                    let elemType, st' = genType resolvedItemName elem emptyState
                    st' |> addDecl(mkAliasDecl resolvedRootName (listType elemType))

        if isNull root then
            ModuleOrNamespaceNode(None, [], Range.Zero)
        else
            let finalState =
                match root with
                | :? JsonObject as o -> emptyState |> ensureRecord resolvedRootName o

                | :? JsonArray as a -> generateRootArray a

                | :? JsonValue as v -> emptyState |> addDecl(mkAliasDecl resolvedRootName (inferPrimitiveType v))

                | _ -> emptyState

            ModuleOrNamespaceNode(None, List.rev finalState.DeclsRev, Range.Zero)
