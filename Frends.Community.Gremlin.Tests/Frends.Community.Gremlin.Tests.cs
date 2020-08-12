using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.Community.Gremlin.Definition;
using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using Newtonsoft.Json;
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
            _input = new ServerConfiguration()
            {
                Host = "40.69.7.172",
                Port = 8182
            };

            Vertex v1 = new Vertex("HR Dictionary","Person");
            
            _vertexForGraph = new GraphQueries.GraphContainer.VertexForGraph();
            _vertexForGraph.VertexId = ""+v1.Id;
            _vertexForGraph.VertexLabel = v1.Label;
            SetUpVertexPropertyQueries(_vertexForGraph);
            
            _graphVertexQueries.Vertices = new GraphQueries.GraphContainer.VertexForGraph[]{_vertexForGraph};
            
            _graphVertexQueries.BuildGraphMessage();
            
            _parameterQueries.GremlinQueryProperties = this.SetUpQueryProperties();//SetUpQueryProperties() == null ? new Options.QueryProperty[] { } : SetUpQueryProperties();
            
            _scriptQueries.GremlinScript = "{AddVertex, g.addV('person').property('id', '1').property('firstName', 'Thomas').property('age', 44)}";
         
            _datasourceConfiguration = new DatasourceConfiguration()
            {
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
        
        //private static Options.QueryProperty[] SetUpQueryProperties()
        private QueryProperty[] SetUpQueryProperties()
        {
            QueryProperty q1 = new QueryProperty();
            q1.Key = "x";
            q1.Value = "10";

            QueryProperty q2 = new QueryProperty();
            q2.Key = "y";
            q2.Value = "10";

            QueryProperty q3 = new QueryProperty();
            q3.Key = "z=x+y";
            q3.Value = "z";

            QueryProperty q4 = new QueryProperty();
            q4.Key = "valuemap";
            q4.Value = "true";

            _gremlinQueryProperties = new [] {q1, q2, q3, q4};
            return _gremlinQueryProperties;
        }

        /// <summary>
        /// You need to Frends.Community.Gremlin.SetPaswordsEnv.ps1 before running unit test, or some other way set environment variables e.g. with GitHub Secrets.
        /// </summary>
        [Test]
        [Ignore("Ignore a test")]
        public async Task GivenKnownGraphQueryWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            IList<string> results = Gremlin.ExecuteVertexQuery(_graphVertexQueries, _datasourceConfiguration, _input, CancellationToken.None).Result;
            
            foreach (dynamic i in results.ToList())
            {
               Console.WriteLine(i.ToString());  
            }
            //List<object> gremlinResponses = results.
            //gremlinResponses.ForEach(i => Console.WriteLine(i.ToString()));
            //gremlinResponses.Select(r => r.Value = JsonConvert.SerializeObject(r.dynamicResultSet.Result)); 
            //gremlinResponses.ForEach(i => Console.WriteLine(i.Value));
            //results.Select(r => r.Value = JsonConvert.SerializeObject(r.dynamicResultSet.Result)); 
            //results.ForEach(i => Console.WriteLine(i.Value));
            //Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.Count());
        }
        
        /// <summary>
        /// You need to Frends.Community.Gremlin.SetPaswordsEnv.ps1 before running unit test, or some other way set environment variables e.g. with GitHub Secrets.
        /// </summary>
        [Test]
        [Ignore("Ignore a test")]
        public async Task GivenKnownPropertyMapWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            Task<IList<string>> response = Task.FromResult(await Gremlin.ExecuteParameterQuery(_parameterQueries, _datasourceConfiguration, _input, CancellationToken.None));
            IList<string> results = response.Result.ToArray();
            foreach (dynamic entry in results)
            {
                Console.WriteLine("Given: entry " + entry);
            }
            Assert.AreEqual(1, results.Count());
        }

        [Test]
        [Ignore("Ignore a test")]
        public async Task GivenKnownScriptQueryWhenExecutingGraphQueryThenValidatedResponseMustBeReturnedFromGraphApi()
        {
            Task<IList<string>> response = Gremlin.ExecuteScriptQuery(_scriptQueries, _datasourceConfiguration, _input, CancellationToken.None);
            IList<string> results = response.Result.ToArray();
            foreach (dynamic entry in results)
            {
                Console.WriteLine("Given: entry " + entry);
            }
            Assert.AreEqual(1, results.Count());
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
