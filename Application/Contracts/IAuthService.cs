using Commons.Classes;
using Models.Entities;
using Models.ViewModels;

namespace Application.Contracts
{
    public interface IAuthService
    {
        Task<APIResponse> Register(User model);
        Task<APIResponse<AuthResponseVm>> Login(AuthRequestVm authRequest);
    }
}
