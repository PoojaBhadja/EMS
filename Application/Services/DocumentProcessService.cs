using Application.Contracts;
using Commons.Classes;
using Commons.Constants;
using Commons.Enums;
using Commons.Helpers;
using Domain;
using Models.Entities;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Net;

namespace Application.Services
{
    public class DocumentProcessService : IDocumentProcessService
    {
        private readonly IRepository _repository;
        private readonly ICommonService _commonService;
        private readonly IBlobService _blobService;
        private readonly IFileDataSource _fileDataSource;
        public DocumentProcessService(IRepository repository, ICommonService commonService, IBlobService blobService, IFileDataSource fileDataSource)
        {
            _repository = repository;
            _commonService = commonService;
            _blobService = blobService;
            _fileDataSource = fileDataSource;
        }
        public async Task<APIResponseVm> DocumentProcessing(DocumentModelVm documentModel, bool isFromBackgroundProcess = false)
        {
            var test = CryptoHelper.Encrypt("https://idf-uks-jbf-api.azurewebsites.net/", "IsfApiEndPoint");//NxHP1qMfEBw4mvJRF1zd0PdQkjS2Op2JGZmcz44TGSqG3aauQIYkFR/9NqdgX1Ka
            var test1 = CryptoHelper.Decrypt(test, "IsfApiEndPoint");
            ClientInformationVm? clientConfigurationData = new();
            Guid DocumentApiLogGuid = Guid.Empty;
            try
            {
            //    clientConfigurationData = (from cedr in _repository.ClientEnterpriseDetailsRepository.DataSet.Where(x => x.EnterpriseId == documentModel.EnterpriseId && x.StoreId == documentModel.StoreId)
            //                               join ccr in _repository.ClientConfigurationRepository.DataSet on cedr.ClientCode equals ccr.ClientCode
            //                               select new ClientInformationVm()
            //                               {
            //                                   ClientCode = ccr.ClientCode,
            //                                   ISFApiEndPoint = "https://localhost:44388/",
            //                                   ISFUserName = CryptoHelper.Decrypt(ccr.IsfWebUserName, Constants.IsfWebUserName),
            //                                   ISFUserPassword = CryptoHelper.Decrypt(ccr.IsfWebPassword, Constants.IsfWebPassword)
            //                               }).FirstOrDefault();

                if (clientConfigurationData == null)
                {
                    var messages = new List<string>() { $"{ErrorMessages.ClientDetailsNotFound}" };
                    await AddUpdateDocumentLog(documentModel,
                      messages: JsonConvert.SerializeObject(messages),
                      status: DocumentApiLogStatus.ClientDetailNotFound,
                      isFromBackgroundProcess: isFromBackgroundProcess);
                    return Helper.CreateApiResponse(ResponseStatus.NotFound, messages);
                }

                //Add Document Process log in DB
                await AddUpdateDocumentLog(documentModel,
                  status: DocumentApiLogStatus.ServiceApiProcessStarted,
                  isFromBackgroundProcess: isFromBackgroundProcess,
                  ClientCode: clientConfigurationData.ClientCode);

                //Call Service Doc api to upload Document
                var response = ApiHelper.PostWithAccessToken(clientConfigurationData, APINameConstants.Process_CreateStagingEntry, JsonConvert.SerializeObject(documentModel));

                //get current status
                DocumentApiLogStatus Status = GetCurrentStatus(response);
                documentModel.Status = Status.ToString();

                var filePath = response.StatusCode == (int)ResponseStatus.Forbidden || response.StatusCode == (int)ResponseStatus.OK ? string.Empty : uploadDocument(documentModel, clientConfigurationData);

                await AddUpdateDocumentLog(documentModel,
                    filePath: filePath,
                    status: Status,
                    messages: JsonConvert.SerializeObject(response),
                    isFromBackgroundProcess: isFromBackgroundProcess);

                //Return Document expire message to DMS
                if (response.StatusCode == (int)ResponseStatus.Forbidden)
                {
                    return Helper.CreateApiResponse(ResponseStatus.DocumentLinkExpire, new List<string>() { $"{ErrorMessages.DocumentLinkExpire}" });
                }

                if (response.StatusCode == (int)ResponseStatus.ApiUnauthorized)
                {
                    return Helper.CreateApiResponse(ResponseStatus.ApiUnauthorized, new List<string>() { $"{ErrorMessages.UnableToAuthorizeAPI}" });
                }

                //delete blob storage if ok status
                if (response.StatusCode == (int)ResponseStatus.OK)
                {
                    DeleteBlobStorageFile(documentModel.FilePath, GetBlobConnectionString(clientConfigurationData));
                }

                return response;
            }
            catch (Exception e)
            {
                await AddUpdateDocumentLog(documentModel,
                    filePath: uploadDocument(documentModel, clientConfigurationData),
                    status: DocumentApiLogStatus.DocumentApiProcessException,
                    messages: JsonConvert.SerializeObject(e.Message),
                    isFromBackgroundProcess: isFromBackgroundProcess);

                return Helper.CreateApiResponse(ResponseStatus.BadRequest, new List<string>() { $"{ErrorMessages.DocumentProcessing_Exception}" + e.Message });
            }
        }

        private async Task AddUpdateDocumentLog(DocumentModelVm documentModel, DocumentApiLogStatus status, string? ClientCode = null, string? messages = null, string? filePath = null, bool isFromBackgroundProcess = false)
        {
            if (documentModel != null)
            {
                documentModel.ClientCode = ClientCode;
                if (documentModel.DocumentApiLogGuid == Guid.Empty)
                {
                    DocumentApiLog log = new()
                    {
                        Status = (int)status,
                        ClientCode = ClientCode,
                        DocumentName = documentModel.DocumentName,
                        MetadataJson = System.Text.Json.JsonSerializer.Serialize(documentModel),
                        ErrorMessage = messages,
                    };
                    documentModel.DocumentApiLogGuid = await _commonService.AddDocumentProcessLogAsync(log);
                }
                else
                {
                    await _commonService.UpdateDocumentProcessLogAsync(documentModel.DocumentApiLogGuid, filePath, (int)status, documentModel.DocumentName, messages, isFromBackgroundProcess);
                }
            }
        }
        private string uploadDocument(DocumentModelVm documentModel, ClientInformationVm clientInformationVm)
        {
            if (documentModel == null)
            {
                return string.Empty;
            }

            try
            {
                var fileName = string.Empty;
                var tempPath = string.Empty;
                var azureConfiguration = GetBlobConnectionString(clientInformationVm);

                if (!string.IsNullOrWhiteSpace(documentModel.FilePath))
                {
                    fileName = documentModel.FilePath.Split(string.Concat(Constants.Documents, "/")).Last();
                    tempPath = _blobService.DownloadDocumentApisDocumentToTempFile(azureConfiguration, documentModel.FilePath);
                }
                else if (!string.IsNullOrWhiteSpace(documentModel.DocumentDownloadLink))
                {
                    fileName = SplitFileName(documentModel.DocumentDownloadLink);
                    tempPath = DownloadFileInTempFolder(fileName, documentModel);
                }

                DeleteBlobStorageFile(documentModel.FilePath, azureConfiguration);
                FileStream fileStream;
                if (!string.IsNullOrEmpty(azureConfiguration.ConnectionString))
                {
                    //uploaded doc in blob storage
                    fileStream = _fileDataSource.Open(tempPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var url = _blobService.UploadDocument(documentModel, fileStream, azureConfiguration);
                    fileStream?.Close();
                    return url;
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private void DeleteBlobStorageFile(string FilePath, AzureConfiguration azureConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                var fileName = FilePath.Split(string.Concat(Constants.Documents, "/")).Last();
                _blobService.DeleteDocument(
                    fileName,
                    azureConfiguration);
            }
        }

        private AzureConfiguration GetBlobConnectionString(ClientInformationVm clientInformationVm)
        {
            var AccessTokenResponse = ApiHelper.GetWithAccessToken<string>(clientInformationVm, APINameConstants.GlobalConfig_GetAzureBlobConnection);
            var azureConfiguration = new AzureConfiguration()
            {
                ConnectionString = AccessTokenResponse.Result
            };

            return azureConfiguration;
        }
        private string DownloadFileInTempFolder(string fileName, DocumentModelVm documentModel)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            var tempPath = Path.GetTempPath() + @"\" + fileName;
            WebClient webClient = new();
            var downloadedPath = documentModel.DocumentDownloadLink;

            try
            {
                webClient.DownloadFile(downloadedPath, tempPath);
            }
            catch (Exception ex)
            {
                tempPath = string.Empty;
            }

            return tempPath;
        }

        private string SplitFileName(string documentDownloadLink)
        {
            var splitData = documentDownloadLink.Split("?");
            string fileName = null;
            if (splitData.Length > 0)
            {
                fileName = Path.GetFileName(splitData[0]);
            }

            return fileName;
        }

        private DocumentApiLogStatus GetCurrentStatus(APIResponseVm response)
        {
            var documentApiLogStatus = Enum.GetValues(typeof(DocumentApiLogStatus)).Cast<DocumentApiLogStatus>().ToList();
            var status = documentApiLogStatus.FirstOrDefault(x => (int)x == response.StatusCode);
            if (status != 0)
            {
                return status;
            }
            else
            {
                //Update Document Process log in DB with api response
                return (response.StatusCode == (int)ResponseStatus.OK
                    ? DocumentApiLogStatus.ServiceApiProcessEnded
                    : (response.StatusCode == (int)ResponseStatus.Forbidden ? DocumentApiLogStatus.DocumentLinkExpire : DocumentApiLogStatus.ServiceApiProcessFailed));
            }
        }

        public List<DocumentModelVm> BackgroundDocumentProcessing()
        {
            //var documentApiLogs = _repository.DocumentApiLogRepository.DataSet.Where(x => (x.Status == (int)DocumentApiLogStatus.ServiceApiProcessFailed || x.Status == (int)DocumentApiLogStatus.DocumentApiProcessException) && x.RetryCount <= 3).Take(2);
            List<DocumentModelVm> documentModelVms = new List<DocumentModelVm>();

            //foreach (var documentApiLog in documentApiLogs)
            //{
            //    DocumentModelVm documentModelVm = JsonConvert.DeserializeObject<DocumentModelVm>(documentApiLog.MetadataJson);
            //    documentModelVm.DocumentApiLogGuid = documentApiLog.DocumentApiLogGuid;
            //    documentModelVm.DocumentApiLogId = documentApiLog.DocumentApiLogId;
            //    documentModelVm.FilePath = documentApiLog.FilePath;
            //    documentModelVms.Add(documentModelVm);
            //}

            return documentModelVms;
        }
    }
}
