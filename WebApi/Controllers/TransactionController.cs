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
    public class TransactionController : AutorizeController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{month:int?}")]
        public async Task<IActionResult> GetAll(int? month)
        {
            return Ok(await _transactionService.GetAll(month));
        }


        [HttpPost]
        public async Task<IActionResult> Post(TransactionRequest model)
        {
            return Ok(await _transactionService.Save(model));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, TransactionRequest model)
        {
            return Ok(await _transactionService.Update(id, model));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return Ok(await _transactionService.Delete(id));
        }
    }
}
