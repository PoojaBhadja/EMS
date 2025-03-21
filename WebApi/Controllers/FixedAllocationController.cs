using Application.Contracts;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.ViewModels;
using Models.ViewModels.Input;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    public class FixedAllocationController : AutorizeController
    {
        private readonly IFixedAllocationService _fixedAllocationService;
        public FixedAllocationController(IFixedAllocationService fixedAllocationService)
        {
            _fixedAllocationService = fixedAllocationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _fixedAllocationService.GetAll());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAllByCardHolderId(Guid id)
        {
            return Ok(await _fixedAllocationService.GetAllByCardHolderId(id));
        }

        [HttpPost]
        public async Task<IActionResult> Post(FixedAllocationRequest model)
        {
            return Ok(await _fixedAllocationService.Save(model));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, FixedAllocationRequest model)
        {
            return Ok(await _fixedAllocationService.Update(id, model));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _fixedAllocationService.Delete(id));
        }
    }
}
