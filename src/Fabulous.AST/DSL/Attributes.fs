namespace Fabulous.AST

open Fabulous.AST.ScalarAttributeDefinitions
open Fabulous.AST.WidgetAttributeDefinitions
open Fabulous.AST.WidgetCollectionAttributeDefinitions

module Attributes =
    let inline defineScalar<'T> name : SimpleScalarAttributeDefinition<'T> =
        let key = AttributeDefinitionStore.getNextKeyForScalar()
        { Key = key; Name = name }

    /// Define an attribute storing a single Widget.
    /// Used for storing the single child of a view
    let inline defineWidget name : WidgetAttributeDefinition =
        let key = AttributeDefinitionStore.getNextKeyForWidget()
        { Key = key; Name = name }

    let inline defineWidgetCollection name : WidgetCollectionAttributeDefinition =
        let key = AttributeDefinitionStore.getNextKeyForWidgetCollection()
        { Key = key; Name = name }
