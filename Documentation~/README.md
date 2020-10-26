# Semantic Traits

## What is Semantic Traits?
Semantic Traits package provides a modeling language to define data schema for labeling and querying runtime objects.

Traits can be used
- To represent semantic concepts attached to objects in the world (Tag)
- As a contract interface in a tool to represent inputs/outputs constraints
- As a domain modeling unit in a state representation (AI Planner)

## Defining models
Traits is an asset-based modeling language. 
These assets can be created via the asset creation menu (Create -> Semantic -> Trait or Enum Definition) or the Create menu from the project window.

### Traits definition
A trait definition is a data schema composed of a unique label and a set of typed properties. Each trait is serialized in an individual asset (scriptable object)
It supports most basic data types, such as int, float, bool, Vector3, or enumerations.

### Enum definition
A enum definition allows to define custom enumerations to be used by properties in a trait definition.

## Configuring the scene
Once you've defined your assets, you can configure your scene by adding traits to individual GameObjects.

### Semantic Object
To attach a trait to a GameObject an authoring component is generated for each trait, and can be added via the classic 'Add Component' method.
In the background this MonoBehaviour is converted into a DOTS entity, to allow faster queries in the world.

### Semantic Query
Semantic query is an API to query for objects tagged with traits in the world at runtime. It's based on EntityQuery system and allows complex custom filters or scorers.
A visual block-programming UI allows a designer to create a query with realtime preview results. It can be added to a GameObject with a 'Semantic Query' component.

## Forums
Discussions about Traits and domain modeling can be found on the forum.
https://forum.unity.com/forums/ai-navigation-previews.122/
