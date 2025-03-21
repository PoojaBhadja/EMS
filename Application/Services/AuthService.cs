using Application.Contracts;
using Azure.Core;
using Commons;
using Commons.Classes;
using Commons.Constants;
using Commons.Enums;
using Commons.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models.Entities;
using Models.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly IRepository _repository;
        private readonly ILogger<AuthService> _logger;
        private readonly ICacheService _cacheService;

        public AuthService(IRepository repository, ILogger<AuthService>? logger, ICacheService cacheService)
        {
            _repository = repository;
            _logger = logger ?? throw new ArgumentException(null, nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentException(null, nameof(cacheService));
        }

        public async Task<APIResponse> Register(User model)
        {
            try
            {
                IList<User>? catchData = GetCachedUsers();
                bool isdublicate = catchData?.Any(x => x.UserName == model.UserName || x.Email == model.Email) ??
                    await _repository.UserRepository.DataSet.AnyAsync(x => x.UserName == model.UserName || x.Email == model.Email);
                if (isdublicate)
                    throw new Exception("User with this username or email already exists.");

                var hashedPassword = CryptoHelper.Encrypt(model.Password);
                bool isValid = CryptoHelper.VerifyPassword(model.Password, hashedPassword);

                model.Password = hashedPassword;
                model.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();
                model.IsActive = false;

                _repository.UserRepository.Create(model);
                await _repository.UserRepository.SaveAsync();

                _cacheService.Remove(CacheKeys.GetAllUsers);

                return APIResponseFactory.Success(ErrorMessages.DataSaved);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the user detail.");
                return APIResponseFactory.Failure(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public async Task<APIResponse<AuthResponseVm>> Login(AuthRequestVm authRequest)
        {
            _cacheService.RemoveAll();
            if (authRequest is null)
            {
                return Helper.CreateApiResponse<AuthResponseVm>(HttpStatusCode.BadRequest, new List<string>() { ErrorMessages.RequestParameterIsNotProper });
            }

            IList<User>? catchData = GetCachedUsers();
            if (catchData == null || catchData?.Count <= 0)
            {
                var data = _repository.UserRepository.DataSet.Where(x => x.IsActive == true).AsNoTracking().
                    Select(g => new User
                    {
                        Email = g.Email,
                        UserName = g.UserName,
                        Password = g.Password,
                        IsActive = g.IsActive
                    })
                    .ToList();
                _cacheService.Set<IList<User>>(CacheKeys.GetAllUsers, data);
            }

            var user = catchData?.FirstOrDefault(x => x.UserName == authRequest.UserName && x.IsActive == true) ??
                await _repository.UserRepository.DataSet.FirstOrDefaultAsync(x => x.UserName == authRequest.UserName && x.IsActive == true);

            if (user == null || !CryptoHelper.VerifyPassword(authRequest.Password, user.Password))
            {
                return APIResponseFactory.Failure<AuthResponseVm>(HttpStatusCode.Unauthorized, "Invalid username or password.");
            }

            return APIResponseFactory.Success<AuthResponseVm>(GenerateToken(user));
        }

        private IList<User>? GetCachedUsers()
        {
            return _cacheService.Get<IList<User>?>(CacheKeys.GetAllUsers);
        }

        private AuthResponseVm GenerateToken(User user)
        {
            try
            {

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Current.JwtSettings.IssuerSigningKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtCustomClaimNames.UserId, user.Id.ToString())};

                var token = new JwtSecurityToken(
                    issuer: AppSettings.Current.JwtSettings.ValidIssuer,
                    audience: AppSettings.Current.JwtSettings.ValidAudience,
                    claims: claims,
                    expires: UtilityHelper.GetIndianTimeZoneDatetime().AddMinutes(AppSettings.Current.JwtSettings.ExpiryMinutes),
                    signingCredentials: credentials);

                return new AuthResponseVm
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(token),
                    expires_in = AppSettings.Current.JwtSettings.ExpiryMinutes
                };
            }
            catch (Exception e)
            {

                throw;
            }

        }
    }
}
