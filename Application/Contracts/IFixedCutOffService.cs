using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;

namespace Application.Contracts
{
    public interface IFixedCutOffService
    {
        Task<APIResponse<IEnumerable<FixedCutOffVm>>> Get();
        Task<APIResponse> Save(FixedCutOffInput model);
        Task<APIResponse> Update(Guid id, FixedCutOffInput model);
        Task<APIResponse> Delete(Guid id);
    }
}
