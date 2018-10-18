using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using DroneRemote.Helpers;
using Microsoft.WindowsAzure.Storage.Auth;

namespace DroneRemote.Services
{
    public static class BlobStorageService
    {
        public static CloudStorageAccount StorageAccount;
        public static CloudBlobClient BlobClient;
        public const string StorageAccountName = "YOUR_STORAGE_ACCOUNT_NAME";
        public const string StorageAccountKey = "YOUR_STORAGE_ACCOUNT_KEY";
        public const string DataContainerName = "YOUR_DATA_CONTAINER_NAME";
        public const string PicturesContainerName = "YOUR_PICTURES_CONTAINER_NAME";
        public static bool isInitialized = false;

        private static void Init()
        {
            if (!isInitialized)
            {
                var credentials = new StorageCredentials(StorageAccountName, StorageAccountKey);
                StorageAccount = new CloudStorageAccount(credentials, true);
                BlobClient = StorageAccount.CreateCloudBlobClient();

                isInitialized = true;
            }
        }

        public static async Task<List<string>> GetBlobList(string containerName)
        {
            if (!isInitialized)
                Init();

            var list = new List<string>();

            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);

            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync("", true , BlobListingDetails.All, 10, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        if(await blob.ExistsAsync())
                            list.Add(blob.Name);
                    }
                }
            } while (token != null);

            return list;
        }

        public static async Task<List<Uri>> GetBlobUriList(string containerName)
        {
            if (!isInitialized)
                Init();

            var list = new List<Uri>();

            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);

            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        if (await blob.ExistsAsync())
                            list.Add(blob.Uri);
                    }
                }
            } while (token != null);

            return list;
        }

        public static async Task<List<TelemetryMessage>> GetData(CloudBlockBlob blobItem)
        {
            var telemetry = new List<TelemetryMessage>();
            if (blobItem == null)
                return telemetry;

            if (!isInitialized)
                Init();

            CloudBlobContainer container = BlobClient.GetContainerReference(blobItem.Container.Name);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobItem.Name);

            string data = await blockBlob.DownloadTextAsync();

            if (string.IsNullOrWhiteSpace(data))
                return telemetry;

            var list =  data.Split().ToList();
            list.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            foreach(var item in list)
            {
                telemetry.Add(JsonConvert.DeserializeObject<TelemetryMessage>(item));
            }

            return telemetry;
        }

        public static async Task<List<TelemetryMessage>> GetData(string containerName, string blobItemName)
        {
            var telemetry = new List<TelemetryMessage>();
            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(blobItemName))
                return telemetry;

            if (!isInitialized)
                Init();

            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobItemName);

            string data = await blockBlob.DownloadTextAsync();

            if (string.IsNullOrWhiteSpace(data))
                return telemetry;

            var list = data.Split().ToList();
            list.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            foreach (var item in list)
            {
                telemetry.Add(JsonConvert.DeserializeObject<TelemetryMessage>(item));
            }

            return telemetry;
        }

        public static async Task<List<TelemetryMessage>> GetData(string containerName, string year, string month, string day)
        {
            if (!isInitialized)
                Init();

            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlobDirectory directory = container.GetDirectoryReference($"{year}-{month}-{day}");

            var files = await directory.ListBlobsSegmentedAsync(null);

            string data = string.Empty;
            var telemetry = new List<TelemetryMessage>();

            if (files.Results.Count() == 0)
                return telemetry;

            foreach (var file in files.Results)
            {
                if (file.GetType() == typeof(CloudBlockBlob))
                {
                    try
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)file;
                        data += await blob.DownloadTextAsync();
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Error during data download: {ex.Message}");
                    }
                }
            }

            var list = data.Split().ToList();
            list.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            foreach (var item in list)
            {
                telemetry.Add(JsonConvert.DeserializeObject<TelemetryMessage>(item));
            }

            return telemetry;
        }

        public static async Task DeleteBlockBlob(string containerName, string blobName)
        {
            if (!isInitialized)
                Init();

            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.DeleteIfExistsAsync();
        }
    }
}
