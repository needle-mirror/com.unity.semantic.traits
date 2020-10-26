# Semantic Traits
Semantic Traits package provides a modeling language to define data schema for labeling and querying runtime objects.

## Traits definition
A trait definition is a data schema composed of a unique label and a set of typed properties.
Each trait is serialized in an individual asset (scriptable object)

It can be used
- To represent semantic concepts attached to objects in the world (Tagging)
- As a contract interface in a tool to represent inputs/outputs (example: a visual-script node that requires an object with 'Locomotion' data)
- As a modeling unit in a custom state representation (AI Planner)

## Traits authoring in the world
To attach a trait to a game object in the scene an authoring component is generated for each trait, and can be added via the classic 'Add Component' method.
In the background this MonoBehaviour is converted into a DOTS entity, to allow faster queries in the world.

## Semantic Query
Semantic query is an API to query for objects tagged with traits in the world at runtime.
It's based on EntityQuery system and allows more complex custom filters or scorers.

A visual block-programming UI allows a designer to create a query via an asset with realtime preview results.
