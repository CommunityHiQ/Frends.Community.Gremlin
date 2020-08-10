using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Frends.Community.Gremlin.Definition;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json;

namespace Frends.Community.Gremlin
{
    
    public class Gremlin
    {
        
        public static async Task<ResultSet<object>> ExecuteParameterQuery(
            [PropertyTab] ServerConfiguration serverConfiguration,
            [PropertyTab] DatasourceConfiguration datasourceConfiguration,
            [PropertyTab] GraphQueries graphQueries,
            CancellationToken cancellationToken)
        {
            // Synchronised scheduling of tasks
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            // Gremlin server configuration
            var gremlinServer = InitializeGremlinServer(serverConfiguration, datasourceConfiguration);

            // Send async queries into the Graph API
            var requests = SendParameterQuery(graphQueries, gremlinServer, scheduler);

            // Response to be returned
            IList<String> response = await LoadResults(requests, scheduler);

            // Load async result
            return (ResultSet<object>) response.Cast<object>();
        }
        
        public static async Task<ResultSet<object>> ExecuteSingleQuery(
            [PropertyTab] ServerConfiguration serverConfiguration,
            [PropertyTab] DatasourceConfiguration datasourceConfiguration,
            [PropertyTab] GraphQueries graphQueries,
            CancellationToken cancellationToken)
        {
            // Synchronised scheduling of tasks
            var scheduler = TaskScheduler.Current;

            // Gremlin server configuration
            var gremlinServer = InitializeGremlinServer(serverConfiguration, datasourceConfiguration);

            // Send async queries into the Graph API
            var requests = SendSingleGraphQuery(graphQueries, gremlinServer, scheduler);

            // Response to be returned
            IList<String> response = await LoadResults(requests, scheduler);

            // Load async result
            return (ResultSet<object>) response.Cast<object>();
        }

        private static GremlinServer InitializeGremlinServer(ServerConfiguration serverConfiguration, DatasourceConfiguration datasourceConfiguration)
        {
            var gremlinServer = new GremlinServer(serverConfiguration.Host, serverConfiguration.Port, enableSsl: true,
                username: "/dbs/" + datasourceConfiguration.Database + "/colls/" + datasourceConfiguration.Collection,
                password: datasourceConfiguration.AuthKey);
            return gremlinServer;
        }

        private static IList<Response> SendSingleGraphQuery(GraphQueries graphQueries,
            GremlinServer gremlinServer, TaskScheduler scheduler)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(),
                GremlinClient.GraphSON2MimeType))
            {
                var results = graphQueries.BuildGraphMessage().Select(q =>
                        new Response
                        {
                            Key = "Frends Task Execution : " + DateTime.Now,
                            dynamicResultSetForResponse = SubmitSingleRequest(gremlinClient, graphQueries, scheduler),
                        }).ToList()
                    .AsParallel().ToList();
                return results;
            }
        }
        
        private static IList<Response> SendParameterQuery(GraphQueries graphQueries, GremlinServer gremlinServer,
            TaskScheduler scheduler)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(),
                GremlinClient.GraphSON2MimeType))
            {
                IList<Response> results = graphQueries.GremlinQueryProperties.Select(q =>
                    SubmitRequest(gremlinClient, q, scheduler).Result).ToList();
                return results;
            }
        }

        private static async Task<IList<string>> LoadResults(IList<Response> asyncResultsFromGraphAPI,
            TaskScheduler taskScheduler)
        {
            if (asyncResultsFromGraphAPI.Count > 0)
            {
                return
                    (await Task.WhenAll(asyncResultsFromGraphAPI.Select(r => r.dynamicResultSetForResponse)))
                    .Select(r => JsonConvert.SerializeObject(r)).ToList();
            }

            return new List<string>();
        }

        private static async Task<ResultSet<dynamic>> SubmitSingleRequest(GremlinClient gremlinClient, GraphQueries graphQueries,
            TaskScheduler taskScheduler)
        {
            try
            {
                return gremlinClient.SubmitWithSingleResultAsync<dynamic>(graphQueries.GremlinScript).Result;
            }
            catch (ResponseException e)
            {
                throw new WebException(
                    $"Request to server failed with status code {(int) e.StatusCode}. Response body: ");
                throw;
            }
            catch (ThreadInterruptedException e)
            {
                if (gremlinClient != null)
                {
                    gremlinClient.Dispose();
                }

                throw new WebException(
                    $"Request to server failed with status code {e}. Response body: ");
                throw;
            }
            finally
            {
                if (gremlinClient != null)
                {
                    gremlinClient.Dispose();
                }
            }
        }

        private static async Task<Response> SubmitRequest(GremlinClient gremlinClient,
            QueryProperty query, TaskScheduler taskScheduler)
        {
            try
            {
                Response response = new Response();
                Task.Factory
                    .StartNew(
                        delegate { response.Key = gremlinClient.SubmitAsync<dynamic>(query.Key).Result.ToString(); },
                        taskScheduler)
                    .ContinueWith(
                        delegate
                        {
                            response.Value = gremlinClient.SubmitAsync<dynamic>(query.Value).Result.ToString();
                        }, taskScheduler);
                return response;
            }
            catch (ResponseException e)
            {
                throw new WebException(
                    $"Request to server failed with status code {(int) e.StatusCode}. Response body: ");
                throw;
            }
            catch (ThreadInterruptedException e)
            {
                if (gremlinClient != null)
                {
                    gremlinClient.Dispose();
                }
                throw new WebException($"Request to server failed with status code {e}. Response body: ");
                throw;
            }
            finally
            {
                if (gremlinClient != null)
                {
                    gremlinClient.Dispose();
                }
            }
        }
        
    }
    
}