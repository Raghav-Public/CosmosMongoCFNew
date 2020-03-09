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
    class BlobUploader
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
                CloudStorageAccount storageAccount = CloudStorageAccount
                                                        .Parse(this.BlobConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.
                                                    GetContainerReference(this.BlobContainer);
                CloudBlockBlob blockBlob = container.
                                            GetBlockBlobReference(Guid.NewGuid().ToString() + ".json");
                blockBlob.UploadText(data);
            }
            catch(Exception exp)
            {
                throw exp;
            }
        }
    }
}
