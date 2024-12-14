(**
---
title: Classes
category: widgets
index: 15
---
*)

(**
# Classes
Classes are types that represent objects that can have properties, methods, and events.
For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-typedefnregularnode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/classes) for a comparison between the two.
*)

(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| TypeDefn(name: string, parameters: WidgetBuilder<Pattern>)                  | Creates a TypeDefnRegularNode AST node with the specified name and parameters. |
| TypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>)          | Creates a TypeDefnRegularNode AST node with the specified name and constructor. |
| TypeDefn(name: string)                        | Creates a TypeDefnRegularNode AST node with the specified name. |
*)

(**
### Modifiers

| Modifier                                   | Description                                                                                     |
|--------------------------------------------|-------------------------------------------------------------------------------------------------|
| toRecursive() | Adds a scalar indicating that the type definition is recursive.                                         |
| toPrivate()   | Sets the accessibility to the type definition to private.                                               |
| toPublic()    | Sets the accessibility of the type definition to public.                                                |
| toInternal()  | Sets the accessibility of the type definition to internal.                                               |
| typeParams(typeParams: WidgetBuilder<TyparDecls>)  | Adds type parameters to the type definition.                                                         |
| xmlDocs(xmlDocs: string list) | Adds XML documentation comments to the type definition.                                                |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds multiple attributes to the type definition.                                                        |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds a single attribute to the type definition.                                                         |
*)

(**
### Usage
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        TypeDefn(
            "MyClass1",
            Constructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat(ConstantPat(Constant("x")), Int())
                          ParameterPat(ConstantPat(Constant("y")), Int()) ]
                    )
                )
            )
        ) {
            DoExpr(AppExpr(" printfn", [ String("%d %d"); Constant("x"); Constant("y") ]))

            Constructor(
                ParenPat(TuplePat([ Constant("0"); Constant("0") ])),
                AppExpr("MyClass1", ParenExpr(TupleExpr([ Constant("0"); Constant("0") ])))
            )
        }
        |> _.triviaBefore(SingleLine("Constructors"))

        TypeDefn("MyClass2", Constructor(ParenPat(TuplePat([ ParameterPat(ConstantPat(Constant("dataIn")), Int()) ])))) {
            LetBindings([ Value("data", "dataIn") ])
            DoExpr(AppExpr(" self.PrintMessage", ConstantUnit()))

            Member(
                "this.PrintMessage()",
                AppExpr("printf", [ String("Creating MyClass2 with Data %d"); Constant("data") ])
            )
        }

        Open("System.IO")

        TypeDefn("Folder", ParenPat(ParameterPat("pathIn", String()))) {
            LetBindings(
                [ Value("path", "pathIn")
                  Value("filenameArray", AppExpr("Directory.GetFiles", ParenExpr("path")))
                      .returnType(Array(String())) ]
            )

            Member(
                "this.FileArray",
                AppExpr(
                    OptVarExpr("Array.map"),
                    [ ParenLambdaExpr("elem", NewExpr("File", ParenExpr(TupleExpr([ "elem"; "this" ]))))
                      ConstantExpr "filenameArray" ]
                )
            )
        }

        TypeDefn(
            "File",
            ParenPat(
                TuplePat(
                    [ ParameterPat("filename", String())
                      ParameterPat("containingFolder", "Folder") ]
                )
            )
        ) {
            LetBindings([ Value("name", "filename") ])
            Member("this.ContainingFolder", "containingFolder")
        }
        |> _.toRecursive()

        Value("folder1", NewExpr("Folder", ParenExpr("C:/")))

        ForEachDoExpr(
            "file",
            "folder1.FileArray",
            AppExpr("printfn", [ ConstantExpr(String("File: %s")); OptVarExpr("file.Name") ])
        )

        TypeDefn(
            "Person2",
            Constructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat(ConstantPat(Constant("name")), String())
                          ParameterPat(ConstantPat(Constant("lastName")), String())
                          ParameterPat(ConstantPat(Constant("?age")), Int()) ]
                    )
                )
            )
        ) {
            Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
        }

        TypeDefn("Person3", ParenPat()) { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
        |> _.typeParams(PostfixList([ "'a"; "'b" ]))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
