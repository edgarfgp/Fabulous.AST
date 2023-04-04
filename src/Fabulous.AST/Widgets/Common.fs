namespace Fabulous.AST

type AccessControl =
    | Public
    | Private
    | Internal

type FieldType =
    | Type of name: string
    | NameAndType of name: string * string
