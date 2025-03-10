namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak

module ModuleDecl =
    let ModuleDecl = Attributes.defineScalar<ModuleDecl> "ModuleDecl"

    let WidgetKey =
        Widgets.register "ModuleDecl" (fun widget ->
            let modeDecl = Widgets.getScalarValue widget ModuleDecl
            modeDecl)

[<AutoOpen>]
module ModuleDeclBuilders =
    type Ast with

        static member private BaseAny(value: ModuleDecl) =
            WidgetBuilder<ModuleDecl>(ModuleDecl.WidgetKey, ModuleDecl.ModuleDecl.WithValue(value))

        /// <summary>Allows you to create a module declaration with the specified TypeDefnRecordNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnRecordNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Record(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnRegularNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnRegularNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Regular(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnEnumNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnEnumNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Enum(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnUnionNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnUnionNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Union(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnAbbrevNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnAbbrevNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Abbrev(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnExplicitNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnExplicitNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Explicit(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnAugmentationNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnAugmentationNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Augmentation(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified TypeDefnDelegateNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<TypeDefnDelegateNode>) =
            let value = ModuleDecl.TypeDefn(TypeDefn.Delegate(Gen.mkOak value))
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified DeclExpr.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<Expr>) =
            let value = ModuleDecl.DeclExpr(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified OpenListNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<OpenListNode>) =
            let value = ModuleDecl.OpenList(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ParsedHashDirectiveNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<HashDirectiveListNode>) =
            let value = ModuleDecl.HashDirectiveList(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ModuleDeclAttributesNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<ModuleDeclAttributesNode>) =
            let value = ModuleDecl.Attributes(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ExceptionDefnNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<ExceptionDefnNode>) =
            let value = ModuleDecl.Exception(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ExternBindingNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<ExternBindingNode>) =
            let value = ModuleDecl.ExternBinding(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified BindingNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<BindingNode>) =
            let value = ModuleDecl.TopLevelBinding(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ModuleAbbrevNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<ModuleAbbrevNode>) =
            let value = ModuleDecl.ModuleAbbrev(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified NestedModuleNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<NestedModuleNode>) =
            let value = ModuleDecl.NestedModule(Gen.mkOak value)
            Ast.BaseAny(value)

        /// <summary>Allows you to create a module declaration with the specified ValNode.</summary>
        static member AnyModuleDecl(value: WidgetBuilder<ValNode>) =
            let value = ModuleDecl.Val(Gen.mkOak value)
            Ast.BaseAny(value)
