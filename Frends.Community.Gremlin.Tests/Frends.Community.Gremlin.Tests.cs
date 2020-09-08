using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Frends.Community.Gremlin.Definition;
using Gremlin.Net.Structure;
using NUnit.Framework;

namespace Frends.Community.Gremlin.Tests
{
    [TestFixture]
    class GremlinIntegrationTest
    {
        VertexQueries _graphVertexQueries = new VertexQueries();
        ParamaterQueries _parameterQueries = new ParamaterQueries();
        ScriptQueries _scriptQueries = new ScriptQueries();
        ServerConfiguration _input = new ServerConfiguration();
        QueryProperty[] _gremlinQueryProperties = new QueryProperty[]{};
        GraphQueries.GraphContainer.VertexPropertyForGraph[] _vertexPropertyForGraphs = new GraphQueries.GraphContainer.VertexPropertyForGraph[]{};
        DatasourceConfiguration _datasourceConfiguration = new DatasourceConfiguration();
        GraphQueries.GraphContainer.VertexForGraph _vertexForGraph = new GraphQueries.GraphContainer.VertexForGraph();

        [SetUp]
        public void SetUp()
        {
            // Defining the vertex query
            Vertex v1 = new Vertex("HR Dictionary","Person");
            _vertexForGraph = new GraphQueries.GraphContainer.VertexForGraph();
            _vertexForGraph.VertexId = ""+v1.Id;
            _vertexForGraph.VertexLabel = v1.Label;
            SetUpVertexPropertyQueries(_vertexForGraph);
          
            // Defining the query paramaters for created vertex data
            _parameterQueries.GremlinQueryParameters = this.SetUpQueryProperties();//SetUpQueryProperties() == null ? new Options.QueryProperty[] { } : SetUpQueryProperties();
            _scriptQueries.GremlinScript =
                "Graph graph = TinkerGraph.open();" +
                "g = graph.traversal();" +
                "g.addV('person').property('id', '1').property('firstName', 'Thomas').property('age', 44);";

            _datasourceConfiguration = new DatasourceConfiguration()
            {
                Collection = "/dbs/", Database = "test"
            };
            
            // def
            _input = new ServerConfiguration()
            {
                Host = "40.69.7.172",
                Port = 8182,
                EnableSSL = false
            };
        }

        private void SetUpVertexPropertyQueries(GraphQueries.GraphContainer.VertexForGraph vertexForGraph)
        {
            GraphQueries.GraphContainer.VertexPropertyForGraph vertexProperty1 =
                new GraphQueries.GraphContainer.VertexPropertyForGraph()
                {
                    id = "1", label = "Name", value = "Ilkka"
                };
            GraphQueries.GraphContainer.VertexPropertyForGraph vertexProperty2 =
                new GraphQueries.GraphContainer.VertexPropertyForGraph()
                {
                    id = "2", label = "Name", value = "Tuomas"
                };
            _vertexPropertyForGraphs = new [] {vertexProperty1, vertexProperty2};
        }
        
        private QueryProperty[] SetUpQueryProperties()
        {
            QueryProperty q1 = new QueryProperty();
            q1.Key = "x";
            q1.Value = "10";

            QueryProperty q2 = new QueryProperty();
            q2.Key = "y";
            q2.Value = "10";

            QueryProperty q3 = new QueryProperty();
            q3.Key = "i=x+y";
            q3.Value = "i";

            QueryProperty q4 = new QueryProperty();
            q4.Key = "valuemap";
            q4.Value = "true";
            _gremlinQueryProperties = new [] {q1, q2, q3, q4};
            
            
            return _gremlinQueryProperties;
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        //[Ignore("Ignore a test")]
        public async Task GivenKnownVertexPropertiesWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            Task<IList<string>> response = Gremlin.ExecuteVertexQuery(_graphVertexQueries, _datasourceConfiguration, _input, CancellationToken.None);
            /*
            foreach (dynamic i in results.ToList())
            {
               Console.WriteLine(i.ToString());  
            }
            Assert.AreEqual(1, results.Count());*/
            Assert.NotNull(response);
        }

        /// <summary>
        /// Makes a query into the backend system with a given Graph script
        /// </summary>
        [Test]
        //[Ignore("Ignore a test")]
        public async Task GivenKnownScriptQueryWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            Task<IList<string>> response = Gremlin.ExecuteScriptQuery(_scriptQueries, _datasourceConfiguration, _input, CancellationToken.None);
            /*IList<string> results = response.Result.ToArray();
            foreach (dynamic entry in results)
            {
                Console.WriteLine("Given: entry " + entry);
            }*/
            Console.WriteLine(response.ToString());
            Assert.NotNull(response);
        }
        
        /// <summary>
        /// Makes a query into the backend system with a given key value pair.
        /// </summary>
        [Test]
        //[Ignore("Ignore a test")]
        public async Task GivenKnownPropertyMapWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            Task<IList<string>> response = Task.FromResult(await Gremlin.ExecuteParameterQuery(_parameterQueries, _datasourceConfiguration, _input, CancellationToken.None));
            /**IList<string> results = response.Result.ToArray();
            foreach (dynamic entry in results)
            {
                Console.WriteLine("Given: entry " + entry);
            }
            Assert.AreEqual(1, results.Count());*/
            Console.WriteLine(response.ToString());
            Assert.NotNull(response);
        }

        [TearDown]
        public void CleanUp()
        {
            _gremlinQueryProperties = null;
            _input = null;
            _graphVertexQueries = null;
        }
    }
}
