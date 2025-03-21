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
    public class CardHolderController : AutorizeController
    {
        private readonly ICardHolderService _cardHolderService;
        private readonly ILogger _logger;
        public CardHolderController(ICardHolderService cardHolderService)
        {
            _cardHolderService = cardHolderService;
        }

        /// <summary>
        /// Get All AccountDetail
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _cardHolderService.GetAll());
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _cardHolderService.GetById(id));
        }

        /// <summary>
        /// Save AccountDetail 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CardHolderInput model)
        {
            return Ok(await _cardHolderService.Save(model));
        }

        /// <summary>
        /// Update AccountDetail 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, CardHolderInput model)
        {
            return Ok(await _cardHolderService.Update(id, model));
        }

        /// <summary>
        /// Delete AccountDetail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _cardHolderService.Delete(id));
        }
    }
}
