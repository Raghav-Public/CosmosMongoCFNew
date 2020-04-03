using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using CosmosDBInteropSchemaDecoder;
using System.Configuration;
using MongoSink;
using Newtonsoft.Json;

namespace CosmosMongoCFNew
{
    class Program
    {
        public static string monitoredEndpoint = ConfigurationManager.AppSettings["monitoredEndpoint"]; 
        public static string monitoredAuthKey = ConfigurationManager.AppSettings["monitoredAuthKey"]; //"nzGClhLPMYExnb2KUZCA05vZPSQ63Y21ktGB4n4U0gdsHD3cCy7ul9A8suRHNXlxzqCQ6tKO2GaDaMRBoyr2yw==";
        public static string monitoredDBName = ConfigurationManager.AppSettings["monitoredDBName"]; //"test";
        public static string monitoredContainerName = ConfigurationManager.AppSettings["monitoredContainerName"]; //"fintr";

        public static string leaseEndpoint = ConfigurationManager.AppSettings["leaseEndpoint"]; //"https://rdsqlsink.documents.azure.com:443/";
        public static string leaseAuthKey = ConfigurationManager.AppSettings["leaseAuthKey"]; //"xnPzWC3Db0vo0Pme7oWlIzUIdCa8fdHq4WHdgtBbhP4JBUHEfFd3LduDF201xQbO4rj18LZai70c53ptondpRg==";
        public static string leaseDBName = ConfigurationManager.AppSettings["leaseDBName"]; //"testdb";
        public static string leaseContainerName = ConfigurationManager.AppSettings["leaseContainerName"];

        public static string destConnectionString = ConfigurationManager.AppSettings["destConnection"];
        public static string destDBName = ConfigurationManager.AppSettings["destDBName"]; //"testdb";
        public static string destContainerName = ConfigurationManager.AppSettings["destContainerName"];
        public static int insertRetries = int.Parse(ConfigurationManager.AppSettings["insertRetries"]);
        public static int maxItems = int.Parse(ConfigurationManager.AppSettings["maxItems"]);
        public static string blobConnectionString = ConfigurationManager.AppSettings["blobConnectionString"];
        public static string blobContainer = ConfigurationManager.AppSettings["blobContainer"];

        public static string delimiter = ConfigurationManager.AppSettings["delimiter"];
        public static string transformationType = ConfigurationManager.AppSettings["transformationType"];
        public static string sourceKeys = ConfigurationManager.AppSettings["sourceKeys"];
        public static string destinationKey = ConfigurationManager.AppSettings["destinationKey"];
        public static string sourceKeyTransformationString = ConfigurationManager.AppSettings["sourceKeyTransformation"];
        public static string swapKeyValues = ConfigurationManager.AppSettings["swapKeyValues"];

        public static CancellationTokenSource source = new CancellationTokenSource();
        
        static void Main(string[] args)
        {
            try
            {
                Program.RunStartFromBeginningChangeFeed().Wait();
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
        public static async Task RunStartFromBeginningChangeFeed()
        {
            try
            {
                CosmosClient leaseClient = new CosmosClient(leaseEndpoint, leaseAuthKey);
                CosmosClient monitoredClient = new CosmosClient(monitoredEndpoint, monitoredAuthKey);

                Container leaseContainer = leaseClient.GetContainer(leaseDBName, leaseContainerName);
                Container monitoredContainer = monitoredClient.GetContainer(monitoredDBName, monitoredContainerName);

                ChangeFeedProcessor changeFeedProcessor = monitoredContainer
                    .GetChangeFeedProcessorBuilder<object>("ChangeFeedProc", Program.HandleChangesAsync)
                        .WithInstanceName(Guid.NewGuid().ToString())
                        .WithLeaseContainer(leaseContainer)
                        .WithStartTime(DateTime.MinValue.ToUniversalTime())
                        .WithMaxItems(maxItems)
                        //.WithPollInterval(TimeSpan.FromSeconds(3))
                        .Build();
                // </StartFromBeginningInitialization>

                Console.WriteLine($"Starting Change Feed Processor with changes since the beginning...");
                await changeFeedProcessor.StartAsync();
                Console.WriteLine("Change Feed Processor started.");

                // Wait random time for the delegate to output all messages after initialization is done
                await Task.Delay(Int32.MaxValue);
                Console.WriteLine("Press any key to continue with the next demo...");
                Console.ReadKey();
                await changeFeedProcessor.StopAsync();
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
                throw;
            }
        }
        static async Task HandleChangesAsync(IReadOnlyCollection<object> changes, CancellationToken cancellationToken)
        {
            MongoSink.MongoSink mongoSink = new MongoSink.MongoSink(destConnectionString, 
                                                                    destDBName,
                                                                    destContainerName,
                                                                    insertRetries,
                                                                    blobConnectionString,
                                                                    blobContainer);
            SourceTransformation sourceTransformation = null;
            if (!string.IsNullOrEmpty(sourceKeyTransformationString))
            {
                try
                {
                    sourceTransformation = JsonConvert.DeserializeObject<SourceTransformation>
                                                                    (sourceKeyTransformationString);
                }
                catch(Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
            Transformation transformation = new Transformation(transformationType,
                sourceKeys,
                destinationKey,
                delimiter,
                sourceTransformation,
                swapKeyValues);

            Console.WriteLine("Received data from the source:" + changes.Count.ToString());
            foreach (object item in changes)
            {
                //Console.WriteLine(item.ToString());
                JObject objects = JObject.Parse(item.ToString());
                objects.Remove("_lsn");
                objects.Remove("_metadata");
                objects.Remove("_id");
                string json = objects.ToString();
                MongoDB.Bson.BsonDocument bdoc = CosmosDbSchemaDecoder.GetBsonDocument(json, false);
                if (transformationType != "NONE")
                {
                    transformation.Execute(bdoc);
                }
                await mongoSink.InsertAsync(bdoc, source.Token);
            }
        }
    }
}



