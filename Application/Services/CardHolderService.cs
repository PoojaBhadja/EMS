using Application.Contracts;
using AutoMapper;
using Commons;
using Commons.Classes;
using Commons.Constants;
using Commons.Enums;
using Commons.Extentions;
using Commons.Helpers;
using Domain;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.ViewModels;
using Models.ViewModels.Input;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Application.Services
{
    public class CardHolderService : ICardHolderService
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardHolderService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Guid currentUserId;

        public CardHolderService(IRepository? repository, IMapper? mapper, ILogger<CardHolderService>? logger,
            ICacheService cacheService)
        {
            _repository = repository ?? throw new ArgumentException(null, nameof(repository));
            _mapper = mapper ?? throw new ArgumentException(null, nameof(mapper));
            _logger = logger ?? throw new ArgumentException(null, nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentException(null, nameof(cacheService));
            if (_repository != null)
            {
                currentUserId = _repository.GetLoggedInUserId();
            }
        }

        public async Task<APIResponse<IList<CardHolder>>> GetAll()
        {
            try
            {
                IList<CardHolder>? cachedData = GetCachedCardHolders();

                if (cachedData != null && cachedData.Any())
                {
                    return APIResponseFactory.Success(cachedData);
                }

                List<CardHolder> dbset = await _repository.CardHolderRepository.DataSet
                    .Where(x => x.IsActive == true && x.CreatedBy == currentUserId).AsNoTracking()
                    .OrderByDescending(x => x.CreatedDate).ToListAsync();

                if (dbset.Any())
                {
                    _cacheService.Set($"{CacheKeys.GetAllCardHolder}_{currentUserId}", dbset);
                    return APIResponseFactory.Success<IList<CardHolder>>(dbset);
                }

                return APIResponseFactory.Failure<IList<CardHolder>>(HttpStatusCode.NoContent, "No active card holders found.");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Exception occurred while fetching all card holders. Message :{exception.Message}");
                throw;
            }
        }

        public async Task<APIResponse<CardHolder>> GetById(Guid id)
        {
            try
            {
                IList<CardHolder>? cachedData = GetCachedCardHolders();
                CardHolder? dbset = cachedData?.FirstOrDefault(x => x.IsActive == true && x.Id == id)
                                 ?? await _repository.CardHolderRepository.DataSet
                                 .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.IsActive == true && x.Id == id && x.CreatedBy == currentUserId);

                return dbset != null
                       ? APIResponseFactory.Success(dbset)
                       : APIResponseFactory.Failure<CardHolder>(HttpStatusCode.NotFound, "Card holder not found.");

            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Exception occurred while fetching card holder by ID.. Message :{exception.Message}");
                throw;
            }
        }


        public async Task<APIResponse> Save(CardHolderInput model)
        {
            try
            {
                ValidateModel(model);

                bool isDuplicate = false;
                IList<CardHolder>? cachedData = GetCachedCardHolders();

                if (cachedData != null && cachedData.Any())
                {
                    isDuplicate = cachedData.Any(x => model.CardHolderName.ToLower() == x.CardHolderName.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDuplicate = await _repository.CardHolderRepository.DataSet.AnyAsync(x => model.CardHolderName.ToLower() == x.CardHolderName.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDuplicate)
                {
                    model.ThrowDublicateException();
                }

                var DbModel = _mapper.Map<CardHolder>(model);
                DbModel.CreatedBy = _repository.GetLoggedInUserId();
                DbModel.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();
                DbModel.IsActive = true;

                _repository.CardHolderRepository.Create(DbModel);
                await _repository.CardHolderRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCardHolder}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataSaved);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Validation failed while saving CardHolder.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the CardHolder.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, "An error occurred while saving the CardHolder.");
            }

        }

        public async Task<APIResponse> Update(Guid id, CardHolderInput model)
        {
            try
            {
                ValidateModel(model);

                CardHolder? existingModel = null;
                IList<CardHolder>? cachedData = GetCachedCardHolders();

                if (cachedData != null && cachedData.Any())
                {
                    existingModel = cachedData.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                }
                else
                {
                    existingModel = await _repository.CardHolderRepository.DataSet
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == currentUserId);
                }

                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.DataNotFound);
                }

                bool isDuplicate = false;
                if (cachedData != null && cachedData.Any())
                {
                    isDuplicate = cachedData.Any(
                    x => x.CardHolderName.ToLower() == model.CardHolderName.ToLower()
                    && x.Id != id && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDuplicate = await _repository.CardHolderRepository.DataSet.AnyAsync(
                    x => x.CardHolderName.ToLower() == model.CardHolderName.ToLower()
                    && x.Id != id && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDuplicate)
                {
                    throw new Exception(ErrorMessages.DublicateDataFound);
                }

                existingModel.CardHolderName = model.CardHolderName;
                existingModel.Balance = model.Balance;
                existingModel.DisplayName = model.DisplayName;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();


                _repository.CardHolderRepository.Update(existingModel);
                await _repository.CardHolderRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCardHolder}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating CardHolder.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        public async Task<APIResponse> Delete(Guid id)
        {
            try
            {

                CardHolder? existingModel = null;
                IList<CardHolder>? cachedData = GetCachedCardHolders();

                if (cachedData != null && cachedData.Any())
                {
                    existingModel = cachedData.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                }
                else
                {
                    existingModel = await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == currentUserId);
                }

                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.RecordNotFound);
                }

                existingModel.IsActive = false;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.CardHolderRepository.Update(existingModel);
                await _repository.CardHolderRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCardHolder}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting CardHolder with ID: {Id}", id);
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        private IList<CardHolder>? GetCachedCardHolders()
        {
            return _cacheService.Get<IList<CardHolder>?>($"{CacheKeys.GetAllCardHolder}_{currentUserId}");
        }

        private static void ValidateModel(CardHolderInput model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "CardHolder model cannot be null.");
            if (string.IsNullOrWhiteSpace(model.CardHolderName))
                throw new ArgumentNullException(nameof(model.CardHolderName), "CardHolderName is required.");
        }
    }
}
