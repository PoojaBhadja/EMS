using Application.Contracts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.ViewModels;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    public class AuthController : AllowAnonymousController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] AuthRequestVm authRequestVm)
        {
            return Ok(await _authService.Login(authRequestVm));
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            return Ok(await _authService.Register(model));
        }
    }
}
