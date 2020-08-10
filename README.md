# Frends.Community.Gremlin

FRENDS Community Task for Echo

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Gremlin/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Gremlin/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Gremlin) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
     - [Gremlin](#Gremlin)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the task via FRENDS UI Task View or you can find the NuGet package from the following NuGet feed
s and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Gremlin

# Tasks

## Gremlin

Repeats message

### Create free form Graph Script Queries with a simple UI

         { "Cleanup",        "g.V().drop()" },
         { "AddVertex 1",    "g.addV('person').property('id', 'thomas').property('firstName', 'Thomas').property('age', 44)" },
         { "AddVertex 2",    "g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)" },
         { "AddVertex 3",    "g.addV('person').property('id', 'ben').property('firstName', 'Ben').property('lastName', 'Miller')" },
         { "AddVertex 4",    "g.addV('person').property('id', 'robin').property('firstName', 'Robin').property('lastName', 'Wakefield')" },
         { "AddEdge 1",      "g.V('thomas').addE('knows').to(g.V('mary'))" },
         { "AddEdge 2",      "g.V('thomas').addE('knows').to(g.V('ben'))" },
         { "AddEdge 3",      "g.V('ben').addE('knows').to(g.V('robin'))" },
         { "UpdateVertex",   "g.V('thomas').property('age', 44)" },
         { "CountVertices",  "g.V().count()" },
         { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
         { "Project",        "g.V().hasLabel('person').values('firstName')" },
         { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
         { "Traverse",       "g.V('thomas').out('knows').hasLabel('person')" },
         { "Traverse 2x",    "g.V('thomas').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
         { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
         { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
         { "CountEdges",     "g.E().count()" },
         { "DropVertex",     "g.V('thomas').drop()" }, 
         }

### Create dynamic Graph Vertex Queries with Citizen Integrator UI

         { "Cleanup",        "g.V().drop()" },
         { "AddVertex 1",    "g.addV('person').property('id', 'thomas').property('firstName', 'Thomas').property('age', 44)" },
         { "AddVertex 2",    "g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)" },
         { "AddVertex 3",    "g.addV('person').property('id', 'ben').property('firstName', 'Ben').property('lastName', 'Miller')" },
         { "AddVertex 4",    "g.addV('person').property('id', 'robin').property('firstName', 'Robin').property('lastName', 'Wakefield')" },
         { "AddEdge 1",      "g.V('thomas').addE('knows').to(g.V('mary'))" },
         { "AddEdge 2",      "g.V('thomas').addE('knows').to(g.V('ben'))" },
         { "AddEdge 3",      "g.V('ben').addE('knows').to(g.V('robin'))" },
         { "UpdateVertex",   "g.V('thomas').property('age', 44)" },
         { "CountVertices",  "g.V().count()" },
         { "Filter",          "g.V().hasLabel('person').has('age', gt(40)).order().by('firstName', decr)" },
         { "Traverse",       "g.V('thomas').out('knows').hasLabel('person')" },
         { "Traverse 2x",    "g.V('thomas').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
         { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
         { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
         { "CountEdges",     "g.E().count()" },
         { "DropVertex",     "g.V('thomas').drop()" }, 
         }

### Create query map for vertex queries with Citizen Integrator UI

| Key | Value | Description | Example |
| -------- | -------- | -------- | -------- |
| Id | 15 | Loads the vertex with a given id. | `Id=15` |


### Returns

List of string responses.

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Replication | `string` | Repeated string. | `Person = xxx` |

Usage:
To fetch result use syntax:

`#result.Replication`

# Building

Clone a copy of the repo

`git clone https://github.com/CommunityHiQ/Frends.Community.Gremlin.git`

Rebuild the project

`msbuild /property:Configuration=Release`

Run Tests

`dotnet test`

Create a NuGet package

`nuget pack ./Frends.Community.Gremlin.nuspec`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ------- | ------- |
| 1.0.0   | Public release of the connector. |