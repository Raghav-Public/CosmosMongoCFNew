using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoSink
{
    public class MongoSink
    {
        public bool IsUpsert { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public int InsertRetries { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        private IMongoCollection<BsonDocument> MongoCollection { get; set; }
        public MongoSink(string connectionString,
                         string databaseName,
                         string collectionName,
                         int insertRetries,
                         string blobConnectionString,
                         string blobContainer,
                         bool isUpsert)
        {
            this.ConnectionString = connectionString;
            this.DatabaseName = databaseName;
            this.CollectionName = collectionName;
            this.InsertRetries = insertRetries;
            this.BlobConnectionString = blobConnectionString;
            this.BlobContainerName = blobContainer;
            this.IsUpsert = isUpsert;
        }

        private void getCollection()
        {
            try
            {
                if (this.MongoCollection == null)
                {
                    var mongoClient = new MongoClient(this.ConnectionString);
                    var database = mongoClient.GetDatabase(this.DatabaseName);
                    this.MongoCollection = database.GetCollection<BsonDocument>(this.CollectionName);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }
        private async Task FindDoc(BsonDocument _doc)
        {
            MongoCollection.FindAsync(new BsonDocument("_id", _doc.GetValue("_id")))
        }
        private async Task insertUpdateDocumentAsync(BsonDocument _doc, CancellationToken cancellationToken)
        {
            bool success = false;
            for (int i = 0; i < InsertRetries; i++) {
                try
                {
                    if (IsUpsert)
                    {
                        await MongoCollection.ReplaceOneAsync(
                            filter: new BsonDocument("_id", _doc.GetValue("_id")),
                            options: new UpdateOptions { IsUpsert = true },
                            replacement: _doc);
                    }
                    else
                    {
                        await MongoCollection.InsertOneAsync(_doc, cancellationToken);
                    }
                    success = true;
                    break;
                }
                catch (Exception exp)
                {
                    if(IsThrottled(exp))
                    {
                        System.Threading.Thread.Sleep(new Random().Next(1, 3));
                        if(i == InsertRetries - 1)
                        {
                            Console.WriteLine("retried for: " + InsertRetries);
                            throw;
                        }
                    }
                    Console.WriteLine(exp.Message);
                }
            }
            if(!success)
            {
                uploadFailedDocumentsToBlob(_doc.ToJson());
            }
        }
        private bool IsThrottled(Exception ex)
        {
            return ex.Message.ToLower().Contains("Request rate is large".ToLower());
        }
        public async Task InsertAsync(BsonDocument _doc, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("trying to insert:" + _doc["_id"]);
                this.getCollection();
                await insertUpdateDocumentAsync(_doc, cancellationToken);
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }
        private void uploadFailedDocumentsToBlob(string failedData)
        {
            try
            {
                BlobUploader blobUploader = new BlobUploader(this.BlobConnectionString, this.BlobContainerName);
                blobUploader.Upload(failedData);
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
                throw;
            }
        }
    }
}
