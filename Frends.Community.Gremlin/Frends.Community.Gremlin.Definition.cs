#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure;

namespace Frends.Community.Gremlin.Definition
{
    
    public class ScriptQueries : GraphQueries
    {
        public string GremlinScript { get; set; }
    }
    
    public class VertexQueries : GraphQueries
    {
        public GraphContainer.VertexForGraph[] Vertices { get; set; }
        
    }

    public class ParamaterQueries
    {
        public QueryProperty[] GremlinQueryProperties { get; set; }
    }
    
    /// <summary>
    /// </summary>
    public class GraphQueries
    {
        /// <summary>
        /// Free pay load query to Graph API server.
        /// </summary>
        /// TODO : make UI easy to use
        /// [UIHint(
        /// { "Cleanup",        "g.V().drop()" },
        /// { "AddVertex 1",    "g.addV('person').property('id', 'thomas').property('firstName', 'Thomas').property('age', 44)" },
        /// { "AddVertex 2",    "g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)" },
        /// { "AddVertex 3",    "g.addV('person').property('id', 'ben').property('firstName', 'Ben').property('lastName', 'Miller')" },
        /// { "AddVertex 4",    "g.addV('person').property('id', 'robin').property('firstName', 'Robin').property('lastName', 'Wakefield')" },
        /// { "AddEdge 1",      "g.V('thomas').addE('knows').to(g.V('mary'))" },
        /// { "AddEdge 2",      "g.V('thomas').addE('knows').to(g.V('ben'))" },
        /// { "AddEdge 3",      "g.V('ben').addE('knows').to(g.V('robin'))" },
        /// { "UpdateVertex",   "g.V('thomas').property('age', 44)" },
        /// { "CountVertices",  "g.V().count()" },
        /// { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
        /// { "Project",        "g.V().hasLabel('person').values('firstName')" },
        /// { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
        /// { "Traverse",       "g.V('thomas').out('knows').hasLabel('person')" },
        /// { "Traverse 2x",    "g.V('thomas').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
        /// { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
        /// { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
        /// { "CountEdges",     "g.E().count()" },
        /// { "DropVertex",     "g.V('thomas').drop()" }, 
        /// }
        //
        //)]

        public string MessageContainer
        {
            get; set;
        }
        
        public IList<GraphContainer> BuildGraphMessage()
        {
            StringBuilder builder = new StringBuilder("g.V().drop()");
            builder.Append("{");
            IList<GraphContainer> graphs = new List<GraphContainer>();
            VertexQueries vertexQueries = new VertexQueries();
            vertexQueries.Vertices?.ToList().ForEach(v =>
            {
                GraphContainer graph = new GraphContainer();
                Vertex vertex = new Vertex(v.VertexId, v.VertexLabel);
                graph.Vertices.Add((vertex));
                //builder.Append("{'Vertex " + vertex.Id + "', g.addV('" + vertex.Label + "')");
                builder.Append(vertex.ToString());
                v.VertexProperties?.ToList().ForEach(p =>
                {
                    VertexProperty vertexProperty = new VertexProperty(p.id, p.label, p.value, vertex);
                    graph.VertexProperties.Add(vertexProperty);
                    //builder.Append(".property('" + vertexProperty.Key + "', '" + vertexProperty.Value + "')");
                    builder.Append(vertexProperty.ToString());
                    builder.Append(v.VertexProperties?.Last() != null ? "" : ",");
                });
                v.Edges?.ToList().ForEach(e =>
                {
                    Vertex fromVertex = new Vertex(graph.Vertices.Select(i => e.fromVertexId == i.Id).Single());
                    Vertex toVertex = new Vertex(graph.Vertices.Select(i => e.toVertexId == i.Id).Single());
                    Edge edge = new Edge(e.id, toVertex, e.label, fromVertex);
                    graph.Edges.Add(edge);
                    //$"e[{Id}][{OutV.Id}-{Label}->{InV.Id}]";
                    builder.Append(edge.ToString());
                    v.Filters.ToList().ForEach(f =>
                    {
                        Property property = new Property(f.label, f.value, vertex);
                        graph.Properties.Add(property);
                        builder.Append(property.ToString());
                        builder.Append(v.VertexProperties?.Last() != null ? "" : ",");
                    });
                    graphs.Add((graph));
                });
                /// { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
                /// { "Project",        "g.V().hasLabel('person').values('firstName')" },
                /// { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
                /// { "Sort",           "g.V().hasLabel('person').has('age', gt(40)).order().by('firstName', decr)" },
                v.Filters?.ToList().ForEach(f =>
                {
                    builder.Append(".haslabel('" + f.label + "')").Append(
                        ".has('" + f.key + "'), " + GraphContainer.FilterForGraph.FilteringOption.GT + "(" +
                        f.value + ")).order().by('" + f.OrderByLabel + "', decr)");
                    builder.Append(v.Filters?.Last() != null ? "" : ",");
                });
                /// { "Traverse",       "g.V('thomas').out('knows').hasLabel('person')" },
                /// { "Traverse 2x",    "g.V('thomas').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
                v.Traversals?.ToList().ForEach(t =>
                {
                    builder.Append(".out('" + t.edge + "')").Append(".hasLabel('" + t.label + ")");
                    builder.Append(v.Traversals?.Last() != null ? "" : ",");
                });
                /// { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
                v.Loops?.ToList().ForEach(l =>
                {
                    builder
                        .Append(
                            ".repeat(out()).until(has('" + l.vertexLabel + ", '" + l.graphColumnValue + "'))")
                        .Append(".path()");
                    builder.Append(v.Loops?.Last() != null ? "" : ",");
                });
                /// { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
                /// { "CountEdges",     "g.E().count()" },
                v.DropEdges?.ToList().ForEach(de =>
                {
                    builder.Append(".outE('" + de.edge + "')").Append(
                        ".where(inV()).has('" + de.graphColumnKey + ", '" + de.graphColumnValue + "').drop()");
                    builder.Append(v.DropEdges?.Last() != null ? "" : ",");
                });
                /// { "DropVertex",     "g.V('thomas').drop()" }, 
                v.DropVertexes?.ToList().ForEach(de => { builder.Append(".drop()"); });
            });
            builder.Append("}");
            this.MessageContainer = builder.ToString();
            Console.Out.WriteLine(this.MessageContainer);
            return graphs;
        }
        
        public class GraphContainer : Graph
        {
            public string Dictionary { get; set; }

            public IList<Vertex> Vertices = new List<Vertex>();

            public IList<VertexProperty> VertexProperties = new List<VertexProperty>();

            public IList<Edge> Edges = new List<Edge>();

            public IList<Property> Properties = new List<Property>();
            
            
            public class VertexForGraph
            {
                public string VertexId { get; set; }

                public string VertexLabel { get; set; }

                public VertexPropertyForGraph[] VertexProperties { get; set; }

                public EdgeForGraph[] Edges { get; set; }
                
                public FilterForGraph[] Filters { get; set; }

                public TraversalForGraph[] Traversals { get; set; }

                public LoopForGraph[] Loops { get; set; }

                public DropEdgeForGraph[] DropEdges { get; set; }

                public DropVertexForGraph[] DropVertexes { get; set; }

                public string Sort { get; set; }

                public string CountEdges { get; set; }

            }

            public class VertexPropertyForGraph
            {
                /*
                public VertexPropertyForGraph(string id, string label, string value)
                {
                    this.id = id;
                    this.label = label;
                    this.value = value;
                }*/

                public string id { get; set; }

                public string label { get; set; }

                public string value { get; set; }
            }

            public class EdgeForGraph
            {
                /*
                public EdgeForGraph(string id, string fromVertexId, string label, string toVertexId)
                {
                    this.id = id;
                    this.fromVertexId = fromVertexId;
                    this.label = label;
                    this.toVertexId = toVertexId;
                }*/

                public string id { get; set; }

                public string fromVertexId { get; set; }

                public string label { get; set; }

                public string toVertexId { get; set; }
            }

            /// { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
            /// { "Project",        "g.V().hasLabel('person').values('firstName')" },
            /// { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
            public class FilterForGraph
            {
                /*
                public FilterForGraph(string label, string key, string value)
                {
                    this.label = label;
                    this.key = key;
                    this.value = value;
                }*/

                public string label { get; set; }
                public string key { get; set; }
                public string value { get; set; }

                public string OrderByLabel { get; set; }

                public enum FilteringOption
                {
                    LS,
                    GT,
                    IS,
                    HAS,
                    DESC,
                    ASC
                }

            }

            public class TraversalForGraph
            {
                public string vertex { get; set; }

                public string edge { get; set; }

                public string label { get; set; }
            }

            public class LoopForGraph
            {
                public string vertexLabel { get; set; }

                public string graphSurrogateColumn { get; set; }
                public string graphColumnValue { get; set; }
            }

            public class DropEdgeForGraph
            {
                public string vertexLabel { get; set; }

                public string edge { get; set; }

                public string graphColumnKey { get; set; }

                public string graphColumnValue { get; set; }
            }

            public class DropVertexForGraph
            {
                public string label { get; set; }
            }
        }
    }
    
    public class QueryProperty
    {
        public string Key { get; set; }
        
        public string Value { get; set; }
    }

    public class DatasourceConfiguration
    {
        /// <summary>
        /// How repeats of input are separated.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(" ")]
        public string Database { get; set; }

        /// <summary>
        /// How repeats of input are separated.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(" ")]
        public string Collection { get; set; }

        /// <summary>
        /// How repeats of input are separated.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(" ")]
        public string Username { get; set; }

        /// <summary>
        /// How repeats of input are separated.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(" ")]
        public string Password { get; set; }

        /// <summary>
        /// How repeats of input are separated.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(" ")]
        public string AuthKey { get; set; }
    }

    public class Response
    {
        public string Key { get; set; }

        public string Value { get; set; }
        
        public Task<ResultSet<dynamic>> dynamicResultSetForQuery { get; set; }

        public Task<ResultSet<dynamic>> dynamicResultSetForResponse { get; set; }
    }

    public class Header
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class ServerConfiguration
    {
        /// <summary>
        /// Number of times input is echoed.
        /// </summary>
        [DefaultValue("8182")]
        [DisplayFormat(DataFormatString = "Text")]
        public int Port { get; set; }

        /// <summary>
        /// The HTTP Method to be used with the request.
        /// </summary>
        // public Method Method { get; set; }
        /// <summary>
        /// The URL with protocol and path. You can include query parameters directly in the url.
        /// </summary>
        [DefaultValue("https://example.org/path/to")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Host { get; set; }

        /// <summary>
        /// List of HTTP headers to be added to the request.
        /// </summary>
        public Header[] Headers { get; set; }
    }
    
}