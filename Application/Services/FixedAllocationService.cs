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
using DocumentFormat.OpenXml.Wordprocessing;

namespace Application.Services
{
    public class FixedAllocationService : IFixedAllocationService
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardHolderService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Guid currentUserId;

        public FixedAllocationService(IRepository? repository, IMapper? mapper, ILogger<CardHolderService>? logger, ICacheService cacheService)
        {
            _repository = repository ?? throw new ArgumentException(null, nameof(repository));
            _mapper = mapper ?? throw new ArgumentException(null, nameof(mapper));
            _logger = logger ?? throw new ArgumentException(null, nameof(logger));
            _cacheService = _cacheService = cacheService ?? throw new ArgumentException(null, nameof(cacheService));
            if (_repository != null)
            {
                currentUserId = _repository.GetLoggedInUserId();
            }
        }

        public async Task<APIResponse<CardSummary>> GetAll()
        {
            try
            {
                List<GroupedFixedAllocation>? dbset;
                IList<CardHolder>? cachedData = GetCachedGetAllCardHolder();
                IList<FixedAllocation>? cachedFixedAllocationData = GetCachedFixedAllocation();

                if (cachedFixedAllocationData == null)
                {
                    UpdateFixedAllocationCatch();
                    cachedFixedAllocationData = GetCachedFixedAllocation();
                }

                if (cachedData == null)
                {
                    UpdateCardHolderCatch();
                    cachedData = GetCachedGetAllCardHolder();
                }

                if (cachedData != null)
                {
                    dbset = dbset = cachedData
                    .Where(ch => ch.IsActive == true)
                    .Select(ch => new GroupedFixedAllocation
                    {
                        CardHolderId = ch.Id,
                        CardHolderName = ch.CardHolderName,
                        DisplayName = ch.DisplayName,
                        TotalBalance = ch.Balance,

                        TotalAllocationAmount = cachedFixedAllocationData?
                            .Where(fa => fa.IsActive == true && fa.CardHolderId == ch.Id && fa.CreatedBy == currentUserId)
                            .DefaultIfEmpty()
                            .Sum(fa => (decimal?)fa?.Amount ?? 0),

                        RemainAmount = ch.Balance - cachedFixedAllocationData?
                            .Where(fa => fa.IsActive == true && fa.CardHolderId == ch.Id && fa.CreatedBy == currentUserId)
                            .DefaultIfEmpty()
                            .Sum(fa => (decimal?)fa?.Amount ?? 0),

                        Allocations = cachedFixedAllocationData?
                            .Where(fa => fa.IsActive == true && fa.CardHolderId == ch.Id && fa.CreatedBy == currentUserId)
                            .Select(fa => new FixedAllocation
                            {
                                Id = fa.Id,
                                Amount = fa.Amount,
                                Name = fa.Name
                            })
                            .ToList()
                    })
                    .OrderByDescending(x => (x.Allocations ?? new List<FixedAllocation>()).Count)
                    .ToList();
                }
                else
                {
                    dbset =
                    await _repository.CardHolderRepository.DataSet
                          .AsNoTracking()
                          .Where(ch => ch.IsActive == true)
                          .Select(ch => new GroupedFixedAllocation
                          {
                              CardHolderId = ch.Id,
                              CardHolderName = ch.DisplayName,
                              DisplayName = ch.DisplayName,
                              TotalBalance = ch.Balance,

                              TotalAllocationAmount = ch.FixedAllocations
                                  .Where(fa => fa.IsActive == true && fa.CreatedBy == currentUserId)
                                  .DefaultIfEmpty()
                                  .Sum(fa => (decimal?)fa.Amount ?? 0),

                              RemainAmount = ch.Balance - ch.FixedAllocations
                                  .Where(fa => fa.IsActive == true && fa.CreatedBy == currentUserId)
                                  .DefaultIfEmpty()
                                  .Sum(fa => (decimal?)fa.Amount ?? 0),

                              Allocations = ch.FixedAllocations
                                  .Where(fa => fa.IsActive == true && fa.CreatedBy == currentUserId)
                                  .Select(fa => new FixedAllocation
                                  {
                                      Id = fa.Id,
                                      Amount = fa.Amount,
                                      Name = fa.Name
                                  })
                                  .ToList()
                          }).OrderByDescending(x => (x.Allocations ?? new List<FixedAllocation>()).Count)
                          .ToListAsync();
                }

                // Aggregate card summary values
                var cardSummary = new CardSummary
                {
                    TotalAllocationAmount = dbset.Sum(d => d.TotalAllocationAmount),
                    TotalBalance = dbset.Sum(d => d.TotalBalance),
                    RemainAmount = dbset.Sum(d => d.RemainAmount),
                    GroupedFixedAllocations = dbset
                };


                return dbset.Any()
                       ? APIResponseFactory.Success<CardSummary>(cardSummary)
                       : APIResponseFactory.Failure<CardSummary>(HttpStatusCode.NoContent, "No active fixed allocation found found.");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Exception occurred while fetching fixed allocation. Message :{exception.Message}");
                throw;
            }
        }

        public async Task<APIResponse<IList<FixedAllocation>>> GetAllByCardHolderId(Guid id)
        {
            try
            {
                IList<FixedAllocation>? cachedFixedAllocationData = GetCachedFixedAllocation();

                var dbset = cachedFixedAllocationData?
                    .Where(x => x.IsActive == true && x.CardHolderId == id && x.CreatedBy == currentUserId)
                    .OrderByDescending(x => x.CreatedDate).ToList() ??

                    await _repository.FixedAllocationRepository.DataSet
                    .Where(x => x.IsActive == true && x.CardHolderId == id && x.CreatedBy == currentUserId)
                    .OrderByDescending(x => x.CreatedDate).ToListAsync();
                return dbset.Any()
                       ? APIResponseFactory.Success<IList<FixedAllocation>>(dbset)
                       : APIResponseFactory.Failure<IList<FixedAllocation>>(HttpStatusCode.NoContent, "No active fixed allocation found found.");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Exception occurred while fetching fixed allocation. Message :{exception.Message}");
                throw;
            }
        }

        public async Task<APIResponse> Save(FixedAllocationRequest model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "model cannot be null.");
                }
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new ArgumentNullException(nameof(model.Name), "FixedAllocation name is required.");
                }

                IList<FixedAllocation>? cachedFixedAllocationData = GetCachedFixedAllocation();

                bool isDuplicate = cachedFixedAllocationData?.Any(x => model.Name.ToLower() == x.Name.ToLower()
                && x.IsActive == true
                && x.CardHolderId == model.CardHolderId && x.CreatedBy == currentUserId) ??

                    await _repository.FixedAllocationRepository.DataSet
                    .AnyAsync(x => model.Name.ToLower() == x.Name.ToLower()
                && x.IsActive == true
                && x.CardHolderId == model.CardHolderId && x.CreatedBy == currentUserId);
                if (isDuplicate)
                {
                    model.ThrowDublicateException();
                }

                var DbModel = _mapper.Map<FixedAllocation>(model);
                DbModel.CreatedBy = _repository.GetLoggedInUserId();
                DbModel.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.FixedAllocationRepository.Create(DbModel);
                await _repository.FixedAllocationRepository.SaveAsync();

                UpdateFixedAllocationCatch();

                return APIResponseFactory.Success(ErrorMessages.DataSaved);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Validation failed while saving FixedAllocation.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the FixedAllocation.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, "An error occurred while saving the FixedAllocation.");
            }

        }

        public async Task<APIResponse> Update(Guid id, FixedAllocationRequest model)
        {
            try
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model), ErrorMessages.RequestParameterIsNotProper);
                }

                var existingModel = _repository.FixedAllocationRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.DataNotFound);
                }

                var isDuplicate = await _repository.FixedAllocationRepository.DataSet.AnyAsync(
                    x => x.Name.ToLower() == model.Name.ToLower()
                    && x.Id != id && x.IsActive == true
                    && x.CardHolderId == model.CardHolderId
                    && x.CreatedBy == currentUserId);

                if (isDuplicate)
                {
                    throw new Exception(ErrorMessages.DublicateDataFound);
                }

                //\ existingModel = _mapper.Map<FixedAllocation>(model);
                existingModel.Name = model.Name;
                //existingModel.CardHolderId = model.CardHolderId;
                existingModel.Amount = model.Amount;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();


                _repository.FixedAllocationRepository.Update(existingModel);
                await _repository.FixedAllocationRepository.SaveAsync();

                UpdateFixedAllocationCatch();

                return APIResponseFactory.Success(ErrorMessages.DataUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating FixedAllocation.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        public async Task<APIResponse> Delete(Guid id)
        {
            try
            {
                var existingModel = _repository.FixedAllocationRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.RecordNotFound);
                }

                existingModel.IsActive = false;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.FixedAllocationRepository.Update(existingModel);
                await _repository.FixedAllocationRepository.SaveAsync();

                UpdateFixedAllocationCatch();

                return APIResponseFactory.Success(ErrorMessages.DataDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting FixedAllocation with ID: {Id}", id);
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        private IList<FixedAllocation>? GetCachedFixedAllocation()
        {
            return _cacheService.Get<IList<FixedAllocation>?>($"{CacheKeys.GetAllFixedAllocation}_{currentUserId}");
        }

        private IList<CardHolder>? GetCachedGetAllCardHolder()
        {
            return _cacheService.Get<IList<CardHolder>?>($"{CacheKeys.GetAllCardHolder}_{currentUserId}");
        }

        public void UpdateFixedAllocationCatch()
        {
            _cacheService.Remove($"{CacheKeys.GetAllFixedAllocation}_{currentUserId}");
            IList<FixedAllocation> data = _repository.FixedAllocationRepository.DataSet
                .Where(x => x.IsActive == true && x.CreatedBy == currentUserId)
                .AsNoTracking()
                .ToList();
            _cacheService.Set($"{CacheKeys.GetAllFixedAllocation}_{currentUserId}", data);
        }

        public void UpdateCardHolderCatch()
        {
            _cacheService.Remove($"{CacheKeys.GetAllCardHolder}_{currentUserId}");
            List<CardHolder> data = _repository.CardHolderRepository.DataSet
                    .Where(x => x.IsActive == true && x.CreatedBy == currentUserId).AsNoTracking()
                    .OrderByDescending(x => x.CreatedDate).ToList();
            _cacheService.Set($"{CacheKeys.GetAllCardHolder}_{currentUserId}", data);
        }
    }
}
