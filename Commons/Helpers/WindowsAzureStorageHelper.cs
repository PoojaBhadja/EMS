using Commons.Classes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.IO.Compression;

namespace Commons.Helpers
{
    public class WindowsAzureStorageHelper
    {
        /// <summary>
        /// Upload files to blob storage in compressed format
        /// </summary>
        /// <param name="config"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="fileStream"></param>
        /// <returns>Compressed size of uploaded file</returns>
        public static string UploadBlob(AzureConfiguration config, string containerName, string blobName, FileStream fileStream, bool compress = true)
        {
            if (!string.IsNullOrWhiteSpace(config.ConnectionString) && CloudStorageAccount.TryParse(config.ConnectionString, out CloudStorageAccount account))
            {
                // Retrieve storage account information from connection string

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config.ConnectionString);

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create a container for organizing blobs within the storage account.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                try
                {
                    // The call below will fail if the sample is configured to use the storage emulator in the connection string, but 
                    // the emulator is not running.
                    // Change the retry policy for this call so that if it fails, it fails quickly.
                    BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                    container.CreateIfNotExistsAsync(requestOptions, null).Wait();
                }
                catch
                {
                    throw;
                }

                // To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate 
                // access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions 
                // to allow public access to blobs in this container.
                container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off }).Wait();

                // Upload a BlockBlob to the newly created container
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                if (compress)
                {
                    using (MemoryStream streamCompressed = new MemoryStream())
                    {
                        using (DeflateStream deflateStream = new DeflateStream(streamCompressed, CompressionMode.Compress))
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            fileStream.CopyTo(deflateStream);
                            deflateStream.Flush();
                            deflateStream.Close();
                        }

                        using (MemoryStream streamOut = new MemoryStream(streamCompressed.ToArray()))
                        {
                            blockBlob.UploadFromStreamAsync(streamOut).Wait();
                            SetCompressedFlagToBlob(blockBlob);
                            //return blockBlob.Properties.Length;
                        }
                    }
                }
                else
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    blockBlob.UploadFromStreamAsync(fileStream).Wait();
                    //return blockBlob.Properties.Length;
                }
                return Convert.ToString(blockBlob.StorageUri?.PrimaryUri);
            }

            return string.Empty;
        }

        /// <summary>
        /// Delete Blob from storage
        /// </summary>
        /// <param name="config"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        public static void DeleteBlob(AzureConfiguration config, string containerName, string filePath)
        {
            if (!string.IsNullOrWhiteSpace(config.ConnectionString) && CloudStorageAccount.TryParse(config.ConnectionString, out CloudStorageAccount account))
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config.ConnectionString);

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create a container for organizing blobs within the storage account.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                try
                {
                    //Get the blob reference using the name
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);

                    //Delete the Blob and also its snapshots if exist
                    blockBlob.DeleteIfExistsAsync().Wait();
                }
                catch
                {
                    throw;
                }
            }
        }

        public static MemoryStream DownloadBlob(AzureConfiguration config, string containerName, string blobName)
        {
            if (!string.IsNullOrWhiteSpace(config.ConnectionString) && CloudStorageAccount.TryParse(config.ConnectionString, out CloudStorageAccount account))
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config.ConnectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                if (container.ExistsAsync().Result)
                {
                    // Retrieve reference to a blob named "photo1.jpg".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    if (blockBlob.ExistsAsync().Result)
                    {
                        MemoryStream comp = new MemoryStream();

                        blockBlob.DownloadToStreamAsync(comp).Wait();

                        if (blockBlob.Metadata.ContainsKey("Compressed"))
                        {
                            MemoryStream decomp = new MemoryStream();

                            comp.Seek(0, SeekOrigin.Begin);
                            using (DeflateStream decompressStream = new DeflateStream(comp, CompressionMode.Decompress))
                            {
                                try
                                {
                                    decompressStream.CopyTo(decomp);
                                }
                                catch (InvalidDataException)
                                {
                                    comp.Seek(0, SeekOrigin.Begin);
                                    comp.CopyTo(decomp);
                                }
                            }
                            return decomp;
                        }
                        return comp;
                    }
                }
            }

            return null;
        }

        public static string DownloadBlobToTempFile(AzureConfiguration config, string containerName, string blobName)
        {
            string filePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                using (MemoryStream ms = DownloadBlob(config, containerName, blobName))
                {
                    if (ms != null)
                    {
                        filePath = Path.GetTempFileName();
                        filePath = Path.ChangeExtension(filePath, Path.GetExtension(blobName));
                        using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            ms.WriteTo(file);
                            file.Close();
                            ms.Close();
                        }
                    }
                }
            }
            return filePath;

        }

        #region Private Methods

        private static void SetCompressedFlagToBlob(CloudBlockBlob blockBlob)
        {
            if (!blockBlob.Metadata.ContainsKey("Compressed"))
            {
                blockBlob.Metadata.Add(new KeyValuePair<string, string>("Compressed", "true"));
                blockBlob.SetMetadataAsync().Wait();
            }
        }

        #endregion
    }
}
