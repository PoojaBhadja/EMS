using Models.ViewModels;

namespace Application.Contracts
{
    public interface IDocumentProcessService
    {
        Task<APIResponseVm> DocumentProcessing(DocumentModelVm documentModel, bool isFromBackgroundProcess = false);
        List<DocumentModelVm> BackgroundDocumentProcessing();
    }
}
