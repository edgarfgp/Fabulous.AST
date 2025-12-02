namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetBinding =
    let ReturnType = Attributes.defineWidget "Type"
    let IsSetter = Attributes.defineScalar<bool> "IsSetter"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "PropertyGetSetBinding" (fun widget ->
            let leadingKeyword =
                if Widgets.getScalarValue widget IsSetter then
                    SingleTextNode.``set``
                else
                    SingleTextNode.``get``

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let inlined =
                Widgets.tryGetScalarValue widget IsInlined
                |> ValueOption.map(fun x -> if x then Some SingleTextNode.``inline`` else None)
                |> ValueOption.defaultValue None

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let returnType = Widgets.tryGetNodeFromWidget widget ReturnType

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

            let pattern =
                Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))

            let parameters =
                Widgets.tryGetScalarValue widget Parameters
                |> ValueOption.defaultValue([ pattern ])
                |> List.ofSeq

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            PropertyGetSetBindingNode(
                inlined,
                attributes,
                accessControl,
                leadingKeyword,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module PropertyGetSetBindingBuilders =
    type Ast with

        static member private BaseGetter(expr: WidgetBuilder<Expr>, ?returnType: WidgetBuilder<Type>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(false)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> PropertyGetSetBinding.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a getter for a property.</summary>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Object3D", UnitPat()) {
        ///            Member(
        ///                "this.Position",
        ///                Getter(ConstantExpr "_position")
        ///            )
        ///        }
        ///    }
        ///}
        /// </code>
        static member Getter(expr: WidgetBuilder<Expr>) = Ast.BaseGetter(expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(ConstantExpr "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.BaseGetter(expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(ConstantExpr "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.BaseGetter(expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Object3D", UnitPat()) {
        ///            Member(
        ///                "this.Position",
        ///                Getter(Constant("_position"))
        ///            )
        ///        }
        ///    }
        ///}
        /// </code>
        static member Getter(expr: WidgetBuilder<Constant>) = Ast.BaseGetter(Ast.ConstantExpr(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("_position"), LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.BaseGetter(Ast.ConstantExpr(expr), returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("_position"), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.BaseGetter(Ast.ConstantExpr(expr), Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Object3D", UnitPat()) {
        ///            Member(
        ///                "this.Position",
        ///                Getter("_position")
        ///            )
        ///        }
        ///    }
        ///}
        /// </code>
        static member Getter(expr: string) = Ast.BaseGetter(Ast.ConstantExpr(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(expr: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseGetter(Ast.ConstantExpr(expr), returnType)

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ ParenPat(ParameterPat("a", Int())) ], ConstantExpr "_position", returnType = LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        ///</code>
        static member Getter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Expr>, ?returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue(parameters |> Seq.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(false)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> PropertyGetSetBinding.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ ParenPat(ParameterPat("a", Int())) ], ConstantExpr "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        ///</code>
        static member Getter(parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Getter(parameters, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], ConstantExpr "_position")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], ConstantExpr "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], ConstantExpr "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>, returnType: string) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], ConstantExpr "_position")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Expr>) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        /// AnonymousModule() {
        ///     TypeDefn("Object3D", UnitPat()) {
        ///         Member(
        ///             "this.Position",
        ///             Getter([ "a" ], ConstantExpr "_position", LongIdent("Vector3"))
        ///         )
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        /// AnonymousModule() {
        ///     TypeDefn("Object3D", UnitPat()) {
        ///         Member(
        ///             "this.Position",
        ///             Getter([ "a" ], ConstantExpr "_position", "Vector3")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameters, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Getter(parameters, Ast.ConstantExpr(expr), returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>, returnType: string)
            =
            Ast.Getter(parameters, Ast.ConstantExpr(expr), Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ Constant("a") ], Constant "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>, returnType: string)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Getter(parameters, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], Constant "_position")
        ///             )
        ///     }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], Constant "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], Constant "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Getter(parameters |> Seq.map Ast.Constant, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], "_position")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: string) =
            Ast.Getter(parameters, Ast.Constant(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], "_position", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: string, returnType: WidgetBuilder<Type>) =
            Ast.Getter(parameters, Ast.Constant(expr), returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter([ "a" ], "_position", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameters: string seq, expr: string, returnType: string) =
            Ast.Getter(parameters, Ast.Constant(expr), Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(
        ///                    ParenPat("index"),
        ///                    IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), returnType = LongIdent("Vector3"))
        ///                )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, ?returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue([ parameter ] |> Seq.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(false)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> PropertyGetSetBinding.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(
        ///                    ParenPat("index"),
        ///                    IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3"))
        ///                )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.Getter(parameter, expr, returnType)

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(
        ///                    ParenPat("index"),
        ///                    IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3")
        ///                )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Getter(parameter, expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(
        ///                     Constant("index"),
        ///                     IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")))
        ///                 )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Getter(Ast.ConstantPat(parameter), expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3")))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.Getter(Ast.ConstantPat(parameter), expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Getter(Ast.ConstantPat(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index"))))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Expr>) =
            Ast.Getter(Ast.Constant(parameter), expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3")))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.Getter(Ast.Constant(parameter), expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Getter(Ast.Constant(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(
        ///                     ParenPat("index"),
        ///                     Constant("ordinals")
        ///                 )
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameter, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), Constant("ordinals"), LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Getter(parameter, Ast.ConstantExpr(expr), returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), Constant("ordinals"), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Getter(parameter, Ast.ConstantExpr(expr), Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), Constant("ordinals"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.Getter(Ast.ConstantPat(parameter), expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), Constant("ordinals"), LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter
            (parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Getter(Ast.ConstantPat(parameter), expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter(Constant("index"), Constant("ordinals"), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Getter(Ast.ConstantPat(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", Constant("ordinals"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Constant>) =
            Ast.Getter(Ast.Constant(parameter), expr)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", Constant("ordinals"), LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Getter(Ast.Constant(parameter), expr, returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", Constant("ordinals"), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Getter(Ast.Constant(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a getter for a property.</summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", "ordinals")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: string) =
            Ast.Getter(parameter, Ast.Constant(expr))

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", "ordinals", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: string, returnType: WidgetBuilder<Type>) =
            Ast.Getter(parameter, Ast.Constant(expr), returnType)

        /// <summary>
        /// Create a getter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the getter.</param>
        /// <param name="expr">The expression to return.</param>
        /// <param name="returnType">The return type of the getter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Getter("index", "ordinals", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Getter(parameter: string, expr: string, returnType: string) =
            Ast.Getter(parameter, Ast.Constant(expr), Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(UnitExpr(), returnType = LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(expr: WidgetBuilder<Expr>, ?returnType: WidgetBuilder<Type>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(true)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> PropertyGetSetBinding.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a setter for a property.</summary>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(UnitExpr(), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Setter(expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("()"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(expr: WidgetBuilder<Constant>) = Ast.Setter(expr |> Ast.ConstantExpr)

        static member Setter(expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Setter(expr |> Ast.ConstantExpr, returnType)

        static member Setter(expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Setter(expr |> Ast.ConstantExpr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("()")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(expr: string) = Ast.Setter(Ast.Constant(expr))

        static member Setter(expr: string, returnType: WidgetBuilder<Type>) =
            Ast.Setter(Ast.Constant(expr), returnType)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ ParenPat(ParameterPat("a", Int())) ], ConstantExpr "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Expr>, ?returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue(parameters |> Seq.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(true)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> PropertyGetSetBinding.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ Constant("a") ], ConstantExpr "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr)

        static member Setter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr, returnType)

        static member Setter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Expr>, returnType: string) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], ConstantExpr "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: WidgetBuilder<Expr>) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr)

        static member Setter(parameters: string seq, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr, returnType)

        static member Setter(parameters: string seq, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ ParenPat(ParameterPat("a", Int())) ], ConstantExpr "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameters, Ast.ConstantExpr(expr))

        static member Setter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Setter(parameters, Ast.ConstantExpr(expr), returnType)

        static member Setter
            (parameters: WidgetBuilder<Pattern> seq, expr: WidgetBuilder<Constant>, returnType: string)
            =
            Ast.Setter(parameters, Ast.ConstantExpr(expr), Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ Constant("a") ], Constant "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ Constant("a") ], Constant "_position &lt;- a", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr, returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ Constant("a") ], Constant "_position &lt;- a", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameters: WidgetBuilder<Constant> seq, expr: WidgetBuilder<Constant>, returnType: string)
            =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.Setter(parameters, expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], Constant "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], Constant "_position &lt;- a", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr, returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], Constant "_position &lt;- a", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Setter(parameters |> Seq.map Ast.Constant, expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], "_position &lt;- a")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: string) =
            Ast.Setter(parameters, Ast.Constant(expr))

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], "_position &lt;- a", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: string, returnType: WidgetBuilder<Type>) =
            Ast.Setter(parameters, Ast.Constant(expr), returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameters">The parameters to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter([ "a" ], "_position &lt;- a", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameters: string seq, expr: string, returnType: string) =
            Ast.Setter(parameters, Ast.Constant(expr), Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index"))))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            Ast.Setter([ parameter ], expr)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3")))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.Setter([ parameter ], expr, returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Setter([ parameter ], expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index"))))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Setter(Ast.ConstantPat(parameter), expr)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3")))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.Setter(Ast.ConstantPat(parameter), expr, returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Setter(Ast.ConstantPat(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index"))))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Expr>) =
            Ast.Setter(Ast.Constant(parameter), expr)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), LongIdent("Vector3")))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.Setter(Ast.Constant(parameter), expr, returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", IndexWithoutDotExpr(Constant("ordinals"), ConstantExpr(Constant("index")), "Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Setter(Ast.Constant(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), Constant("ordinals[index]"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameter, Ast.ConstantExpr(expr))

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), Constant("ordinals[index]"), LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Setter(parameter, Ast.ConstantExpr(expr), returnType)

        /// <summary>
        /// Create a setter for a property.
        /// </summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(ParenPat("index"), Constant("ordinals[index]"), "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Setter(parameter, Ast.ConstantExpr(expr), Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), Constant("ordinals[index]"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.Setter(Ast.ConstantPat(parameter), expr)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), Constant("ordinals[index]") LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter
            (parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.Setter(Ast.ConstantPat(parameter), expr, returnType)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter(Constant("index"), Constant("ordinals[index]") "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Setter(Ast.ConstantPat(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", Constant("ordinals[index]"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Constant>) =
            Ast.Setter(Ast.Constant(parameter), expr)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", Constant("ordinals[index]") LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Setter(Ast.Constant(parameter), expr, returnType)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", Constant("ordinals[index]") "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Setter(Ast.Constant(parameter), expr, Ast.LongIdent(returnType))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", "ordinals[index]")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: string) =
            Ast.Setter(parameter, Ast.Constant(expr))

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", "ordinals[index]", LongIdent("Vector3"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: string, returnType: WidgetBuilder<Type>) =
            Ast.Setter(parameter, Ast.Constant(expr), returnType)

        /// <summary>Create a setter for a property.</summary>
        /// <param name="parameter">The parameter to the setter.</param>
        /// <param name="expr">The expression to set.</param>
        /// <param name="returnType">The return type of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Object3D", UnitPat()) {
        ///             Member(
        ///                 "this.Position",
        ///                 Setter("index", "ordinals[index]", "Vector3")
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Setter(parameter: string, expr: string, returnType: string) =
            Ast.Setter(parameter, Ast.Constant(expr), Ast.LongIdent(returnType))

type PropertyGetSetBindingModifiers =
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.IsInlined.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<PropertyGetSetBindingNode>, value: WidgetBuilder<Type>) =
        this.AddWidget(PropertyGetSetBinding.ReturnType.WithValue(value.Compile()))

    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<PropertyGetSetBindingNode>, value: string) =
        PropertyGetSetBindingModifiers.returnType(this, Ast.LongIdent(value))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<PropertyGetSetBindingNode>, values: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(
            PropertyGetSetBinding.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<PropertyGetSetBindingNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        PropertyGetSetBindingModifiers.attributes(this, [ attribute ])
