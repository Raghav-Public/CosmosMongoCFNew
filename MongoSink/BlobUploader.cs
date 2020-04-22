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
    public class BlobUploader
    {
        public string BlobConnectionString { get; set; }
        public string BlobContainer { get; set; }

        public BlobUploader(string blobConnectionString, string blobContainer)
        {
            this.BlobConnectionString = blobConnectionString;
            this.BlobContainer = blobContainer;
        }

        public void Upload(String data)
        {
            try
            {
                string filename = Guid.NewGuid().ToString() + ".json";
                Console.WriteLine("writing failed data to:" + filename);
                /*CloudStorageAccount storageAccount = CloudStorageAccount
                                                        .Parse(this.BlobConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.
                                                    GetContainerReference(this.BlobContainer);
                                                    */
                CloudBlobContainer container = GetBlobContainer();
                container.CreateIfNotExists();
                CloudBlockBlob blockBlob = container.
                                            GetBlockBlobReference(filename);
                blockBlob.UploadText(data, Encoding.UTF8);
                Console.WriteLine("wrote to blob");
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp);
                throw exp;
            }
        }
        private CloudBlobContainer GetBlobContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount
                                                    .Parse(this.BlobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.
                                                GetContainerReference(this.BlobContainer);
            return container;
        }
        public List<Uri> GetAllBlobs()
        {
            List<Uri> blobUrls = new List<Uri>();
            CloudBlobContainer container = GetBlobContainer();
            var blobs = container.ListBlobs();
            foreach(var blob in blobs)
            {
                blobUrls.Add(blob.Uri);
            }
            return blobUrls;
        }
        public async Task<string> Download(Uri blobUri)
        {
            CloudBlobContainer container = GetBlobContainer();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobUri.ToString());
            return await blockBlob.DownloadTextAsync();
        }
    }
}
