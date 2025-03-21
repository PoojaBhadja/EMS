using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;

namespace Application.Contracts
{
    public interface IFixedAllocationService
    {
        Task<APIResponse<CardSummary>> GetAll();
        Task<APIResponse<IList<FixedAllocation>>> GetAllByCardHolderId(Guid id);
         Task<APIResponse> Save(FixedAllocationRequest model);
        Task<APIResponse> Update(Guid id, FixedAllocationRequest model);
        Task<APIResponse> Delete(Guid id);

    }
}
