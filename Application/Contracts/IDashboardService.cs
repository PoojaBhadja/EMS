using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Contracts
{
    public interface IDashboardService
    {
        Task<APIResponse<DashboardVm>> GetDashboardSummary(FilterDataVm filterDataVm);
    }
}
