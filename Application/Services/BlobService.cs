
using Application.Contracts;
using Commons.Classes;
using Commons.Constants;
using Commons.Helpers;
using Models.ViewModels;

namespace Application.Services
{
    public partial class BlobService : IBlobService
    {
        #region Properties

        #endregion

        #region Constructors

        public BlobService()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Upload document to blob storage in compressed format
        /// </summary>
        /// <param name="document">Document meta data</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="compress">Flag to enable compression before uploading the document.</param>
        /// <returns>Compressed size of uploaded document</returns>
        public string UploadDocument(object document, FileStream fileStream, AzureConfiguration config, bool compress = true)
        {
            return WindowsAzureStorageHelper.UploadBlob(config, GetContainerName(), GetBlobName(document), fileStream, compress);
        }

        public void DeleteDocument(string filePath, AzureConfiguration config)
        {
            WindowsAzureStorageHelper.DeleteBlob(config, GetContainerName(), filePath);
        }

        public string DownloadDocumentApisDocumentToTempFile(AzureConfiguration config, string link)
        {
            var path = link.Split(string.Concat("documents" + " / ")).Last();
            return WindowsAzureStorageHelper.DownloadBlobToTempFile(config, "documents", path);
        }
        #endregion

        #region Private Methods

        private string GetContainerName()
        {
            return "documents";
        }

        private string GetBlobName(object document) //clientcode
        {
            //if (document is DocumentModelVm)
            //{
            //    DocumentModelVm doc = document as DocumentModelVm;
            //    var filePath = $"{Constants.DocumentApisFailedDocument}/{doc.ClientCode}/{doc.Status}/{DateTime.UtcNow:ddMMyyyyhhmmsstt}{GetFileExtention(doc.DocumentDownloadLink)}";
            //    return filePath;
            //}

            return string.Empty;
        }

        private string GetFileExtention(string Link)
        {
            var splitData = string.IsNullOrWhiteSpace(Link) ? new string[0] : Link.Split("?");
            if (splitData.Length > 0)
            {
                return Path.GetExtension(splitData[0]);
            }

            return string.Empty;
        }
        #endregion
    }
}
