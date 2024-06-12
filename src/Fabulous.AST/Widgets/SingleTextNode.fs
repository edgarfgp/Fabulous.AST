namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type SingleTextNode =
    static member inline Create(idText: string) = SingleTextNode(idText, Range.Zero)

[<RequireQualifiedAccess>]
module SingleTextNode =
    let lessThan = SingleTextNode.Create "<"
    let greaterThan = SingleTextNode.Create ">"
    let equals = SingleTextNode.Create "="
    let comma = SingleTextNode.Create ","
    let ``type`` = SingleTextNode.Create "type"
    let leftAttribute = SingleTextNode.Create "[<"
    let rightAttribute = SingleTextNode.Create ">]"
    let leftCurlyBrace = SingleTextNode.Create "{"
    let rightCurlyBrace = SingleTextNode.Create "}"
    let leftParenthesis = SingleTextNode.Create "("
    let rightParenthesis = SingleTextNode.Create ")"
    let ``abstract`` = SingleTextNode.Create "abstract"
    let ``interface`` = SingleTextNode.Create "interface"
    let ``class`` = SingleTextNode.Create "class"
    let ``end`` = SingleTextNode.Create "end"
    let ``with`` = SingleTextNode.Create "with"
    let ``member`` = SingleTextNode.Create "member"
    let rightArrow = SingleTextNode.Create "->"
    let lefArrow = SingleTextNode.Create "<-"
    let star = SingleTextNode.Create "*"
    let ``if`` = SingleTextNode.Create "if"
    let ``elif`` = SingleTextNode.Create "elif"
    let ``then`` = SingleTextNode.Create "then"
    let ``else`` = SingleTextNode.Create "else"
    let ``let`` = SingleTextNode.Create "let"
    let ``public`` = SingleTextNode.Create "public"
    let ``private`` = SingleTextNode.Create "private"
    let ``internal`` = SingleTextNode.Create "internal"
    let ``inline`` = SingleTextNode.Create "inline"
    let ``namespace`` = SingleTextNode.Create "namespace"
    let ``module`` = SingleTextNode.Create "module"
    let colon = SingleTextNode.Create ":"
    let minus = SingleTextNode.Create "-"
    let caret = SingleTextNode.Create "^"
    let divOp = SingleTextNode.Create "/"
    let ``new`` = SingleTextNode.Create "new"
    let ``lazy`` = SingleTextNode.Create "lazy"
    let ``null`` = SingleTextNode.Create "null"
    let ``struct`` = SingleTextNode.Create "struct"
    let empty = SingleTextNode.Create ""
    let ``static`` = SingleTextNode.Create "static"
    let ``override`` = SingleTextNode.Create "override"
    let leftQuotation = SingleTextNode.Create "<@"
    let rightQuotation = SingleTextNode.Create "@>"
    let leftBracket = SingleTextNode.Create "["
    let rightBracket = SingleTextNode.Create "]"
    let rightArray = SingleTextNode.Create "|]"
    let leftArray = SingleTextNode.Create "[|"
    let ``match`` = SingleTextNode.Create "match"
    let bar = SingleTextNode.Create "|"
    let leftCurlyBraceWithBar = SingleTextNode.Create "{|"
    let rightCurlyBraceWithBar = SingleTextNode.Create "|}"
    let underscore = SingleTextNode.Create "_"
    let ``as`` = SingleTextNode.Create "as"
    let doubleColon = SingleTextNode.Create "::"
    let isInstance = SingleTextNode.Create ":?"
    let forwardSlash = SingleTextNode.Create "/"
    let backSlash = SingleTextNode.Create "\""
    let ``mutable`` = SingleTextNode.Create "mutable"
    let ``val`` = SingleTextNode.Create "val"
    let measure = SingleTextNode.Create "Measure"
    let ``extern`` = SingleTextNode.Create "extern"
    let hash = SingleTextNode.Create "#"
    let ``in`` = SingleTextNode.Create "in"
    let letBang = SingleTextNode.Create "let!"
    let andBang = SingleTextNode.Create "and!"
    let ``use`` = SingleTextNode.Create "use"
    let ``for`` = SingleTextNode.Create "for"
    let ``do`` = SingleTextNode.Create "do"
    let arrow = SingleTextNode.Create "->"
    let ``while`` = SingleTextNode.Create "while"
    let get = SingleTextNode.Create "get"
    let set = SingleTextNode.Create "set"
    let ``and`` = SingleTextNode.Create "and"
    let ``inherit`` = SingleTextNode.Create "inherit"
    let power = SingleTextNode.Create "^"
    let dot = SingleTextNode.Create(".")
    let ampersand = SingleTextNode.Create "&"
    let ``begin`` = SingleTextNode.Create("begin")
    let ``fun`` = SingleTextNode.Create("fun")
    let ``function`` = SingleTextNode.Create("function")
    let ``or`` = SingleTextNode.Create("or")
    let ``..`` = SingleTextNode.Create("..")
