
using Commons.Classes;

namespace Application.Contracts
{
    public interface IBlobService
    {
        string UploadDocument(object document, FileStream fileStream, AzureConfiguration config, bool compress = true);
        void DeleteDocument(string filePath, AzureConfiguration config);
        string DownloadDocumentApisDocumentToTempFile(AzureConfiguration config, string link);
    }
}

