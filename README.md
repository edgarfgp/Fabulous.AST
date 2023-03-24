# Fabulous.AST

Welcome to the Fabulous.AST, an Abstract Syntax Tree (AST) Domain Specific Language (DSL) written in F#!

Fabulous.AST is designed to provide a simple and expressive way to represent code as a tree of nodes. This makes it easier to manipulate and analyze code programmatically. With Fabulous.AST, you can create and manipulate ASTs using F# syntax and functions.

Let's take a look at an example AST in Fabulous.AST:

```fsharp
AnonymousModule() { 
    Let("x", "12")
}
```
produces the following code:

```fsharp
let x = 12
```
In this example, we have an anonymous module that contains a single let binding, which assigns the value "12" to the variable "x". The AST is represented as a series of nested function calls, starting with AnonymousModule() and ending with Let("x", "12").

Using Fabulous.AST, you can easily create and manipulate ASTs like this one using F# functions. For example, you can add new nodes to the AST, modify existing nodes, or traverse the AST to perform analysis or transformation tasks.

Fabulous.AST is a powerful tool for anyone who works with code and wants to automate or streamline their development workflow. Whether you're a compiler writer, a code generator, or just someone who wants to write better code faster, Fabulous.AST can help you achieve your goals.