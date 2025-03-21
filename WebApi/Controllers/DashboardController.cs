using Application.Contracts;
using Application.Services;
using DocumentFormat.OpenXml.Bibliography;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models.ViewModels.Input;
using System.IO;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    public class DashboardController : AutorizeController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpPost("GetDashboardSummary")]
        public async Task<IActionResult> GetDashboardSummary(FilterDataVm filterDataVm)
        {
            return Ok(await _dashboardService.GetDashboardSummary(filterDataVm));
        }
    }
}
