using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Contracts
{
    public interface ITransactionService
    {
        Task<APIResponse<IList<Transaction>>> GetAll(int? Month);
        Task<APIResponse> Save(TransactionRequest requestData);
        Task<APIResponse> Update(Guid id, TransactionRequest requestData);
        Task<APIResponse> Delete(Guid id);
      
    }
}
