using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;

namespace Application.Contracts
{
    public interface ICardHolderService
    {
        Task<APIResponse<IList<CardHolder>>> GetAll();
        Task<APIResponse<CardHolder>> GetById(Guid id);
        Task<APIResponse> Save(CardHolderInput model);
        Task<APIResponse> Update(Guid id, CardHolderInput model);
        Task<APIResponse> Delete(Guid id);
    }
}
