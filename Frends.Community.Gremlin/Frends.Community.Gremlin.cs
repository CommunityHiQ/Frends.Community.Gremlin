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

        public static async Task<IList<string>> ExecuteParameterQuery(
            [PropertyTab] ParamaterQueries parameterQueries,
            [PropertyTab] DatasourceConfiguration datasourceConfiguration,
            [PropertyTab] ServerConfiguration serverConfiguration,
            CancellationToken cancellationToken)
        {
            // Synchronised scheduling of tasks
            var scheduler = TaskScheduler.Current;

            // Gremlin server configuration
            var gremlinServer = InitializeGremlinServer(serverConfiguration, datasourceConfiguration);

            // Send async queries into the Graph API
            var requests = SendParameterQuery(parameterQueries, gremlinServer, scheduler);

            // Response to be returned
            IList<String> responses = await LoadResults(requests, scheduler);

            // Load async result
            return responses;
        }

        public static async Task<IList<string>> ExecuteScriptQuery(
            [PropertyTab] ScriptQueries graphScriptQueries,
            [PropertyTab] DatasourceConfiguration datasourceConfiguration,
            [PropertyTab] ServerConfiguration serverConfiguration,
            CancellationToken cancellationToken)
        {

            // Synchronised scheduling of tasks
            var scheduler = TaskScheduler.Current;

            // Gremlin server configuration
            var gremlinServer = InitializeGremlinServer(serverConfiguration, datasourceConfiguration);

            // Send async queries into the Graph API
            var requests = SendSingleGraphQuery(graphScriptQueries, gremlinServer, scheduler);

            // Response to be returned
            IList<String> response = await LoadResults(requests, scheduler);

            // Load async result
            return response;
            //return (ResultSet<object>) response.Cast<object>();
        }

        public static async Task<IList<string>> ExecuteVertexQuery(
            [PropertyTab] VertexQueries graphScriptQueries,
            [PropertyTab] DatasourceConfiguration datasourceConfiguration,
            [PropertyTab] ServerConfiguration serverConfiguration,
            CancellationToken cancellationToken)
        {
            // Synchronised scheduling of tasks
            var scheduler = TaskScheduler.Current;

            // Gremlin server configuration
            var gremlinServer = InitializeGremlinServer(serverConfiguration, datasourceConfiguration);

            // Send async queries into the Graph API
            var requests = SendVertexGraphQuery(graphScriptQueries, gremlinServer, scheduler);

            // Response to be returned
            IList<String> response = await LoadResults(requests, scheduler);

            // Load async result
            return response;
            //return (ResultSet<object>) response.Cast<object>();
        }

        private static GremlinServer InitializeGremlinServer(ServerConfiguration serverConfiguration,
            DatasourceConfiguration datasourceConfiguration)
        {
            var gremlinServer = new GremlinServer(
                serverConfiguration.Host, serverConfiguration.Port,
                enableSsl: serverConfiguration.EnableSSL,
                username: datasourceConfiguration.Username, //datasourceConfiguration.Collection,
                password: datasourceConfiguration.AuthKey);
            Console.Out.WriteLine("Gremlin API URI : " + gremlinServer.Uri);
            return gremlinServer;
        }

        private static IList<Response> SendSingleGraphQuery(ScriptQueries graphScriptQueries,
            GremlinServer gremlinServer, TaskScheduler scheduler)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON3Reader(), new GraphSON3Writer(),
                GremlinClient.DefaultMimeType))
            {
                var results = graphScriptQueries.GremlinScript.Select(q =>
                        new Response
                        {
                            //Key = "Frends Task Execution : " + DateTime.Now,
                            dynamicResultSetForResponse =
                                SubmitSingleRequest(gremlinClient, graphScriptQueries, scheduler),
                        }).ToList()
                    .AsParallel().ToList();
                return results;
            }
        }

        private static IList<Response> SendVertexGraphQuery(VertexQueries vertexQueries,
            GremlinServer gremlinServer, TaskScheduler scheduler)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON3Reader(), new GraphSON3Writer(),
                GremlinClient.DefaultMimeType))
            {
                vertexQueries.BuildGraphMessage(vertexQueries);
                var results = vertexQueries.MessageContainer.Select(q =>
                        new Response
                        {
                            //Key = "Frends Task Execution : " + DateTime.Now,
                            dynamicResultSetForResponse = SubmitVertexRequest(gremlinClient, vertexQueries, scheduler),
                        }).ToList()
                    .AsParallel().ToList();
                return results;
            }
        }

        private static IList<Response> SendParameterQuery(ParamaterQueries graphQueries, GremlinServer gremlinServer,
            TaskScheduler scheduler)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON3Reader(), new GraphSON3Writer(),
                GremlinClient.DefaultMimeType))
            {
                IList<Response> results = graphQueries.GremlinQueryParameters.Select(q =>
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
                    (await Task.WhenAll(
                        asyncResultsFromGraphAPI.Select(r => r.dynamicResultSetForResponse)))
                    .Select(r => JsonConvert.SerializeObject(r)).ToList();
            }

            return new List<string>();
        }

        private static async Task<ResultSet<dynamic>> SubmitVertexRequest(GremlinClient gremlinClient,
            VertexQueries vertexQueries,
            TaskScheduler taskScheduler)
        {
            try
            {
                return await gremlinClient.SubmitWithSingleResultAsync<dynamic>(vertexQueries.MessageContainer);
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

        private static async Task<ResultSet<dynamic>> SubmitSingleRequest(GremlinClient gremlinClient,
            ScriptQueries scriptQueries,
            TaskScheduler taskScheduler)
        {
            try
            {
                return await gremlinClient.SubmitWithSingleResultAsync<dynamic>(scriptQueries.GremlinScript);
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
                        delegate {  response.Key = gremlinClient.SubmitAsync<dynamic>(query.Key); },
                        taskScheduler)
                    .ContinueWith(
                        delegate
                        {
                            response.Value = gremlinClient.SubmitAsync<dynamic>(query.Value);
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