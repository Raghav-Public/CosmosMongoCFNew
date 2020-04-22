using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MongoSink
{
    public class BlobToCosmos
    {
        public string BlobConnectionString { get; set; }
        public string BlobContainer { get; set; }
        public string MongoConnectionString { get; set; }
        public string MongoDatabase { get; set; }
        public string MongoCollection { get; set; }

        public bool IsUpdate { get; set; }
        public BlobUploader BlobUploader { get; set; }
        public BlobToCosmos(string blobConnection,
                            string blobContainer,
                            string mongoConnection,
                            string mongoDatabase,
                            string mongoCollection
                            )
        {
            this.BlobConnectionString = blobConnection;
            this.MongoConnectionString = mongoConnection;
            this.BlobContainer = blobContainer;
            this.MongoDatabase = mongoDatabase;
            this.MongoCollection = mongoCollection;
            this.BlobUploader = new BlobUploader(BlobConnectionString, BlobContainer);
        }
        
        public async void Execute()
        {
            try
            {
                List<Uri> allBlobs = BlobUploader.GetAllBlobs();   
                foreach(Uri blobUri in allBlobs)
                {
                    string blobText = await DownloadBlob(blobUri);
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        private async Task<string> DownloadBlob(Uri blobUri)
        {
            return await BlobUploader.Download(blobUri);
        }

        private void Insert(string data)
        {

        }
    }
}
