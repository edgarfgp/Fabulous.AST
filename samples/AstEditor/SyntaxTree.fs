namespace AstEditor

open System.Text
open FSharp.Compiler.Syntax

/// Renders the untyped F# syntax tree (from FCS) as a compact, human-readable tree — the AST
/// inspector's "debug view" of what the DSL builds. It summarises the high-level structure
/// (modules, type definitions, fields/cases, bindings, top-level expressions) rather than
/// dumping every node, and is a hand-written walk (no reflection) so the labels stay meaningful.
module SyntaxTree =

    /// The rendered tree plus, for each line, an optional "locate hint" — a salient name to
    /// find in the DSL so clicking a tree line can highlight the code that produced it.
    type Tree =
        { Text: string
          Hints: string option[] }

    /// A node in the tree: a label, an optional locate hint, and children.
    type private Node =
        { Label: string
          Hint: string option
          Children: Node list }

    let private leaf label =
        { Label = label
          Hint = None
          Children = [] }

    let private leafH label hint =
        { Label = label
          Hint = Some hint
          Children = [] }

    let private node label children =
        { Label = label
          Hint = None
          Children = children }

    let private nodeH label hint children =
        { Label = label
          Hint = Some hint
          Children = children }

    let private identText(id: Ident) = id.idText

    let private longText(ids: LongIdent) =
        ids |> List.map identText |> String.concat "."

    let private synLongText(SynLongIdent(ids, _, _)) = longText ids

    /// A shallow, readable rendering of a type (field/parameter types).
    let rec private typeText(t: SynType) : string =
        match t with
        | SynType.LongIdent lid -> synLongText lid
        | SynType.App(typeName = tn; typeArgs = args) ->
            let argText = args |> List.map typeText |> String.concat ", "
            $"{typeText tn}<{argText}>"
        | SynType.Fun(argType = a; returnType = b) -> $"{typeText a} -> {typeText b}"
        | SynType.Tuple _ -> "tuple"
        | SynType.Array(elementType = e) -> $"{typeText e}[]"
        | SynType.Var(typar = SynTypar(id, _, _)) -> "'" + identText id
        | SynType.Paren(innerType = inner) -> typeText inner
        | _ -> "_"

    /// The bound name of a let binding's head pattern.
    let rec private patText(p: SynPat) : string =
        match p with
        | SynPat.Named(ident = SynIdent(id, _)) -> identText id
        | SynPat.LongIdent(longDotId = lid) -> synLongText lid
        | SynPat.Typed(pat = inner) -> patText inner
        | SynPat.Paren(pat = inner) -> patText inner
        | SynPat.Wild _ -> "_"
        | _ -> "_"

    /// A literal's rendered value.
    let private constText(c: SynConst) : string =
        match c with
        | SynConst.String(text = s) -> "\"" + s + "\""
        | SynConst.Int32 n -> string n
        | SynConst.Bool b -> (if b then "true" else "false")
        | SynConst.Double d -> string d
        | SynConst.Single f -> string f
        | SynConst.Char ch -> "'" + string ch + "'"
        | SynConst.Unit -> "()"
        | _ -> "const"

    let private isUnit(e: SynExpr) =
        match e with
        | SynExpr.Const(constant = SynConst.Unit) -> true
        | _ -> false

    /// The function-name label at the head of an application spine (e.g. "Record").
    let rec private headLabel(e: SynExpr) : string =
        match e with
        | SynExpr.Ident id -> identText id
        | SynExpr.LongIdent(longDotId = lid) -> synLongText lid
        | SynExpr.App(funcExpr = f) -> headLabel f
        | SynExpr.TypeApp(expr = inner)
        | SynExpr.Paren(expr = inner) -> headLabel inner
        | _ -> "App"

    /// Render an expression as a node tree — deep enough to show the structure of the DSL
    /// (application calls, computation-expression `{ }` blocks, lists, tuples, literals) so the
    /// AST inspector is a real explorer of the selected code.
    let rec private exprNode(e: SynExpr) : Node =
        match e with
        | SynExpr.Paren(expr = inner)
        | SynExpr.Typed(expr = inner)
        | SynExpr.TypeApp(expr = inner) -> exprNode inner
        | SynExpr.Const(constant = c) ->
            // A string literal's hint is its unquoted text (so clicking `"Name"` locates the
            // DSL `Field("Name", …)`); other literals locate by their rendered value.
            match c with
            | SynConst.String(text = s) -> leafH ("\"" + s + "\"") s
            | _ -> leaf(constText c)
        | SynExpr.Ident id -> leaf(identText id)
        | SynExpr.LongIdent(longDotId = lid) -> leaf(synLongText lid)
        | SynExpr.Tuple(exprs = exprs) -> node "tuple" (exprs |> List.map exprNode)
        | SynExpr.ArrayOrList(exprs = exprs) -> node "[ ]" (exprs |> List.map exprNode)
        | SynExpr.ArrayOrListComputed(expr = body) -> node "[ ]" (seqNodes body)
        | SynExpr.ComputationExpr(expr = body) -> node "{ }" (seqNodes body)
        | SynExpr.App _ ->
            // `a |> b |> c` (the DSL's common tail) flattens to a "|>" chain; otherwise show
            // the function head with its (tuple-flattened) arguments.
            match pipeChain e with
            | Some stages -> node "|>" (stages |> List.map exprNode)
            | None ->
                let head, args = appSpine e
                node ("App " + headLabel head) (appArgNodes args)
        | SynExpr.Lambda(body = body) -> node "lambda" [ exprNode body ]
        | SynExpr.IfThenElse(ifExpr = c; thenExpr = t) -> node "if" [ exprNode c; exprNode t ]
        | SynExpr.Sequential _ -> node "seq" (seqNodes e)
        | _ -> leaf "expr"

    and private appSpine(e: SynExpr) : SynExpr * SynExpr list =
        let rec go acc e =
            match e with
            | SynExpr.App(funcExpr = f; argExpr = a) -> go (a :: acc) f
            | _ -> e, acc

        go [] e

    and private appArgNodes(args: SynExpr list) : Node list =
        args
        |> List.filter(isUnit >> not)
        |> List.collect(fun a ->
            // Flatten a parenthesised tuple call `f(a, b)` into separate argument nodes.
            match a with
            | SynExpr.Paren(expr = SynExpr.Tuple(exprs = exprs)) -> exprs |> List.map exprNode
            | _ -> [ exprNode a ])

    and private seqNodes(e: SynExpr) : Node list =
        match e with
        | SynExpr.Sequential(expr1 = a; expr2 = b) -> seqNodes a @ seqNodes b
        | SynExpr.YieldOrReturn(expr = inner)
        | SynExpr.YieldOrReturnFrom(expr = inner) -> seqNodes inner
        | _ -> [ exprNode e ]

    and private pipeChain(e: SynExpr) : SynExpr list option =
        let isPipe e =
            match e with
            | SynExpr.Ident id -> id.idText = "op_PipeRight"
            | _ -> false

        match e with
        | SynExpr.App(funcExpr = SynExpr.App(funcExpr = op; argExpr = lhs); argExpr = rhs) when isPipe op ->
            let left =
                match pipeChain lhs with
                | Some xs -> xs
                | None -> [ lhs ]

            Some(left @ [ rhs ])
        | _ -> None

    let private fieldNode(SynField(idOpt = idOpt; fieldType = ft)) =
        let name = idOpt |> Option.map identText |> Option.defaultValue "_"
        leafH $"Field {name} : {typeText ft}" name

    let private unionCaseNode(SynUnionCase(ident = SynIdent(id, _); caseType = caseType)) =
        let name = identText id

        match caseType with
        | SynUnionCaseKind.Fields(_ :: _ as fields) -> nodeH $"UnionCase {name}" name (fields |> List.map fieldNode)
        | _ -> leafH $"UnionCase {name}" name

    let private typeDefnNode(SynTypeDefn(typeInfo = SynComponentInfo(longId = lid); typeRepr = repr)) =
        let name = longText lid

        match repr with
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Record(recordFields = fields)) ->
            nodeH $"Record {name}" name (fields |> List.map fieldNode)
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Union(unionCases = cases)) ->
            nodeH $"Union {name}" name (cases |> List.map unionCaseNode)
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Enum(cases = cases)) ->
            nodeH
                $"Enum {name}"
                name
                (cases
                 |> List.map(fun (SynEnumCase(ident = SynIdent(id, _))) -> leafH (identText id) (identText id)))
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.TypeAbbrev(rhsType = ty)) ->
            leafH $"Abbrev {name} = {typeText ty}" name
        | _ -> leafH $"Type {name}" name

    let private bindingNode(SynBinding(headPat = pat)) =
        let name = patText pat
        leafH $"Let {name}" name

    let rec private declNodes(decl: SynModuleDecl) : Node list =
        match decl with
        | SynModuleDecl.Open(target = SynOpenDeclTarget.ModuleOrNamespace(longId = lid)) ->
            [ leaf $"open {synLongText lid}" ]
        | SynModuleDecl.Open _ -> [ leaf "open …" ]
        | SynModuleDecl.Types(typeDefns = tds) -> tds |> List.map typeDefnNode
        | SynModuleDecl.Let(bindings = bs) -> bs |> List.map bindingNode
        | SynModuleDecl.Expr(expr = e) -> [ exprNode e ]
        | SynModuleDecl.NestedModule(moduleInfo = SynComponentInfo(longId = lid); decls = decls) ->
            [ node $"module {longText lid}" (decls |> List.collect declNodes) ]
        | _ -> []

    let private moduleNode(SynModuleOrNamespace(longId = lid; decls = decls; kind = kind)) =
        let label =
            match kind, lid with
            | SynModuleOrNamespaceKind.AnonModule, _ -> "module (anonymous)"
            | SynModuleOrNamespaceKind.DeclaredNamespace, _ -> $"namespace {longText lid}"
            | _, [] -> "module"
            | _, _ -> $"module {longText lid}"

        node label (decls |> List.collect declNodes)

    /// Render a node tree with ├─ / └─ connectors, collecting one locate-hint per line so the
    /// tree text and the hint array line up index-for-index.
    let private render(root: Node) : Tree =
        let sb = StringBuilder()
        let hints = ResizeArray<string option>()
        sb.Append(root.Label).Append('\n') |> ignore
        hints.Add root.Hint

        let rec go (prefix: string) (n: Node) =
            let count = List.length n.Children

            n.Children
            |> List.iteri(fun i child ->
                let isLast = i = count - 1
                let branch = if isLast then "└─ " else "├─ "
                sb.Append(prefix).Append(branch).Append(child.Label).Append('\n') |> ignore
                hints.Add child.Hint
                let childPrefix = prefix + (if isLast then "   " else "│  ")
                go childPrefix child)

        go "" root

        { Text = sb.ToString().TrimEnd('\n')
          Hints = hints.ToArray() }

    /// Summarise a parsed input as a syntax tree (text + per-line locate hints). Signature
    /// files (not produced by the samples) get a short placeholder.
    let summarize(input: ParsedInput) : Tree =
        match input with
        | ParsedInput.ImplFile(ParsedImplFileInput(contents = modules)) ->
            match modules with
            // A bare expression (e.g. a selection like `Oak() { … }`) renders without the
            // anonymous-module wrapper, so the tree starts at the code you selected.
            | [ SynModuleOrNamespace(
                    kind = SynModuleOrNamespaceKind.AnonModule; decls = [ SynModuleDecl.Expr(expr = e) ]) ] ->
                render(exprNode e)
            | [ single ] -> render(moduleNode single)
            | _ -> render(node "ParsedInput" (modules |> List.map moduleNode))
        | ParsedInput.SigFile _ ->
            { Text = "// (signature file)"
              Hints = [| None |] }
