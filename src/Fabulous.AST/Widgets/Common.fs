namespace Fabulous.AST

type AccessControl =
    | Public
    | Private
    | Internal

type FieldType =
    | Named of name: string
    | NameAndType of name: string * string
