(**
---
title: Attributes, XML Docs and Comments
category: widgets
index: 21
---
*)

(**
# Attributes, XML Docs and Comments

## Contents
- [Overview](#Overview)
- [Attributes](#Attributes)
- [Attribute Arguments](#Attribute-Arguments)
- [Multiple Attributes](#Multiple-Attributes)
- [Attribute Targets](#Attribute-Targets)
- [Assembly-Level Attributes](#Assembly-Level-Attributes)
- [XML Documentation](#XML-Documentation)
- [Structured XML Documentation](#Structured-XML-Documentation)
- [Comments](#Comments)
- [Block Comments](#Block-Comments)
- [Conditional Compilation](#Conditional-Compilation)
- [Blank Lines](#Blank-Lines)
- [Hash Directives](#Hash-Directives)
- [Backtick Identifiers and Triple-Quoted Strings](#Backtick-Identifiers-and-Triple-Quoted-Strings)

## Overview
Most declarations can carry metadata that does not change what the code does: attributes, XML documentation and comments.
Fabulous.AST adds them with modifiers such as `attribute`, `attributes`, `xmlDocs`, `triviaBefore` and `triviaAfter`, which work on values, functions, types, members and other declarations.

## Attributes
Create an attribute with `Attribute(name)` and attach it with the `attribute` modifier:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Value("MaxRetries", Int(3)).attribute(Attribute("Literal"))

        Record("Config") {
            Field("Url", String())
            Field("Timeout", Int())
        }
        |> _.attribute(Attribute("CLIMutable"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attribute Arguments
Pass a constant or an expression as the second argument; a single constant generates the parenthesis-free form `[<Obsolete "...">]`, while `ParenExpr` and `TupleExpr` produce an argument list, including named arguments:
*)

Oak() {
    AnonymousModule() {
        Function("oldParse", ParenPat(ParameterPat("s", String())), AppExpr("int", "s"))
            .attribute(Attribute("Obsolete", String("Use Parser.parse instead")))

        Function("legacyParse", ParenPat(ParameterPat("s", String())), AppExpr("int", "s"))
            .attribute(Attribute("Obsolete", ParenExpr(String("Use Parser.parse instead"))))

        TypeDefn("TagAttribute", ParenPat(ParameterPat("name", String()))) {
            InheritUnit("System.Attribute")
            Member("_.Name", ConstantExpr("name"))
        }
        |> _.attribute(
            Attribute(
                "AttributeUsage",
                ParenExpr(
                    TupleExpr(
                        [ ConstantExpr("AttributeTargets.Class")
                          InfixAppExpr("AllowMultiple", "=", "true") ]
                    )
                )
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Note that a tuple must be wrapped in `ParenExpr`; without it the attribute is printed as `[<A "x", B = y>]`, which is not valid F#.

## Multiple Attributes
Use `attributes` with a list to put several attributes in one attribute list:
*)

Oak() {
    AnonymousModule() {
        Open("Xunit")

        Function("parsesNumbers", UnitPat(), AppExpr("Assert.Equal", ParenExpr(TupleExpr([ Int(42); Int(42) ]))))
            .attributes(
                [ Attribute("Fact")
                  Attribute("Trait", ParenExpr(TupleExpr([ String("Category"); String("Unit") ]))) ]
            )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attribute Targets
`AttributeTarget(name, target)` and `AttributeTarget(name, argument, target)` prefix the attribute with a target such as `return`, `field`, `property` or `assembly`.
Here a `return:` target marshals the result of a native function:
*)

Oak() {
    AnonymousModule() {
        Open("System.Runtime.InteropServices")

        ExternBinding("bool", "CloseHandle", ExternBindingPat("nativeint", "handle"))
            .attributes(
                [ Attribute("DllImport", ParenExpr(String("kernel32.dll")))
                  AttributeTarget("MarshalAs", ParenExpr(Constant("UnmanagedType.Bool")), "return") ]
            )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Assembly-Level Attributes
Assembly attributes must be attached to a `do` declaration; `ModuleDeclAttribute` creates that declaration and `AttributeTarget(..., "assembly")` supplies the attribute:
*)

Oak() {
    AnonymousModule() {
        Open("System.Runtime.CompilerServices")

        ModuleDeclAttribute(UnitExpr())
            .attributes(
                [ AttributeTarget("InternalsVisibleTo", String("MyLib.Tests"), "assembly")
                  AttributeTarget("InternalsVisibleTo", String("MyLib.Benchmarks"), "assembly") ]
            )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## XML Documentation
`xmlDocs` with a list of strings writes each string as a `///` line exactly as given:
*)

Oak() {
    AnonymousModule() {
        Value("DefaultPort", Int(8080)).xmlDocs([ "The port used when none is configured." ])

        Value("DefaultHost", String("localhost"))
            .xmlDocs(
                [ "<summary>The host used when none is configured.</summary>"
                  "<remarks>Never null.</remarks>" ]
            )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Structured XML Documentation
`Summary` builds an XML doc node that wraps its text in `<summary>` tags and escapes `<`, `>` and `&`; chain `parameters`, `returnInfo` and `exceptionInfo` to add `<param>`, `<returns>` and `<exception>` tags:
*)

Oak() {
    AnonymousModule() {
        Function(
            "divide",
            [ ParenPat(ParameterPat("a", Int())); ParenPat(ParameterPat("b", Int())) ],
            InfixAppExpr("a", "/", "b")
        )
            .xmlDocs(
                Summary("Divides a by b & rounds towards zero.")
                    .parameters([ ("a", "The dividend."); ("b", "The divisor.") ])
                    .returnInfo("The quotient.")
                    .exceptionInfo("System.DivideByZeroException", "Thrown when b is zero.")
            )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Comments
Comments are trivia attached to a node: `triviaBefore(SingleLine(...))` adds a `//` comment on the line above, and `triviaAfter(LineCommentAfterSourceCode(...))` adds one at the end of the line.
Both modifiers also accept a list:
*)

Oak() {
    AnonymousModule() {
        Value("retries", Int(3))
            .triviaBefore(
                [ SingleLine("How many times to retry a failed request.")
                  SingleLine("Keep this low.") ]
            )

        Value("timeout", Int(30)).triviaAfter(LineCommentAfterSourceCode("seconds"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Block Comments
`BlockComment(text, newlineBefore, newlineAfter)` creates a `(* ... *)` comment; pass `true, true` to put the delimiters on their own lines.
Before a declaration the comment always ends with a line break, since a declaration that starts after a comment on the same line is not valid F#.
Without the flags, a comment before an expression stays on the same line:
*)

Oak() {
    AnonymousModule() {
        Value("version", String("1.0.0"))
            .triviaBefore(BlockComment("Generated by MyGenerator. Do not edit.", true, true))

        Value("delay", ConstantExpr(Int(500)).triviaBefore(BlockComment("ms")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Conditional Compilation
`Directive` adds a preprocessor line such as `#if`, `#else` or `#endif` before or after a node:
*)

Oak() {
    AnonymousModule() {
        Value("logLevel", String("Debug")).triviaBefore(Directive("#if DEBUG")).triviaAfter(Directive("#else"))

        Value("logLevel", String("Warning")).triviaAfter(Directive("#endif"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Blank Lines
`Newline()` adds an empty line, which is useful for separating groups of declarations:
*)

Oak() {
    AnonymousModule() {
        Value("width", Int(800))
        Value("height", Int(600))

        Value("title", String("My App")).triviaBefore([ Newline(); SingleLine("Window title") ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Hash Directives
Hash directives are declarations: `NoWarn` creates `#nowarn` and `HashDirective(name, arguments)` creates any other directive, such as `#r` or `#load` in a script.
See also [Hash Directives](Modules.html#Hash-Directives) on the Modules page.
*)

Oak() {
    AnonymousModule() {
        HashDirective("r", String("nuget: FSharp.Data"))
        HashDirective("load", String("Helpers.fsx"))
        NoWarn(String("40"))

        Open("FSharp.Data")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Backtick Identifiers and Triple-Quoted Strings
Names that are not valid identifiers are wrapped in double backticks, which is common for test names; `TripleQuotedString` produces a `"""..."""` literal that can contain quotes:
*)

Oak() {
    AnonymousModule() {
        Open("System.Text.Json")
        Open("Xunit")

        Function(
            "``serializes a person to JSON``",
            UnitPat(),
            AppExpr(
                "Assert.Equal",
                ParenExpr(
                    TupleExpr(
                        [ ConstantExpr(TripleQuotedString("{\"name\":\"Ada\"}"))
                          AppExpr("JsonSerializer.Serialize", ParenExpr(Constant("{| name = \"Ada\" |}"))) ]
                    )
                )
            )
        )
            .attribute(Attribute("Fact"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
