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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml.Office2010.Excel;
using Azure.Core;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Guid currentUserId;

        public TransactionService(IRepository repository, IMapper mapper, ILogger<TransactionService> logger, ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger ?? throw new ArgumentException(null, nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentException(null, nameof(cacheService));
            if (_repository != null)
            {
                currentUserId = _repository.GetLoggedInUserId();
            }
        }

        public async Task<APIResponse<IList<Transaction>>> GetAll(int? month)
        {
            try
            {
                IList<Transaction>? cachedData = GetCachedTransaction();
                month = month == null ? DateTime.UtcNow.Month : month;
                var dbset = cachedData?.Where(x => x.IsActive == true && x.TransactionDate.Month == month && x.CreatedBy == currentUserId)
                    .OrderByDescending(x => x.CreatedDate).ToList() ??

                    await _repository.TransactionRepository.DataSet.
                            Where(x => x.IsActive == true && x.CreatedBy == currentUserId)
                            .AsNoTracking()
                            .Include(x => x.Category)
                            .Include(x => x.SubCategory)
                            .Include(x => x.CardHolder)
                            .OrderByDescending(x => x.CreatedDate).ToListAsync();

                if (cachedData == null)
                {
                    _cacheService.Set($"{CacheKeys.GetAllTransaction}_{currentUserId}", dbset);
                }

                dbset = dbset.Where(x => x.TransactionDate.Month == month && x.CreatedBy == currentUserId).ToList();

                return dbset.Any()
                      ? APIResponseFactory.Success<IList<Transaction>>(dbset)
                      : APIResponseFactory.Failure<IList<Transaction>>(HttpStatusCode.NoContent, "No active transaction found.");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<APIResponse> Save(TransactionRequest model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }
                bool isDuplicate = false;
                IList<Transaction>? cachedData = GetCachedTransaction();

                if (cachedData != null && cachedData.Any())
                {
                    isDuplicate = cachedData.Any(x => x.CategoryId == model.CategoryId
                    && x.SubCategoryId == model.SubCategoryId
                    && x.CardHolderId == model.CardHolderId
                    && x.Amount == model.Amount
                    && x.TransactionType == model.TransactionType
                    && x.TransactionDate == model.TransactionDate
                    && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDuplicate = _repository.TransactionRepository.DataSet
                    .Any(x => x.CategoryId == model.CategoryId
                    && x.SubCategoryId == model.SubCategoryId
                    && x.CardHolderId == model.CardHolderId
                    && x.Amount == model.Amount
                    && x.TransactionType == model.TransactionType
                    && x.TransactionDate == model.TransactionDate
                    && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDuplicate)
                {
                    throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
                }

                var dbModel = _mapper.Map<Transaction>(model);
                dbModel.TransactionDate = UtilityHelper.GetIndianTimeZoneDatetime(dbModel.TransactionDate);
                dbModel.CreatedBy = _repository.GetLoggedInUserId();
                dbModel.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.TransactionRepository.Create(dbModel);
                await _repository.TransactionRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllTransaction}_{currentUserId}");

                //increas or dicreas amount in main balance
                if (model.IsPaid)
                {
                    var cardHolderExistingModel =
                        await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == model.CardHolderId && x.CreatedBy == currentUserId);
                    if (cardHolderExistingModel is null)
                    {
                        throw new Exception(ErrorMessages.CardHolderNotFound);
                    }
                    if (model.TransactionType == (int)CategoryType.Income)
                    {
                        cardHolderExistingModel.Balance += model.Amount;
                    }
                    else if (model.TransactionType == (int)CategoryType.Expence
                        || model.TransactionType == (int)CategoryType.Investment)
                    {
                        cardHolderExistingModel.Balance -= model.Amount;
                    }

                    _repository.CardHolderRepository.Update(cardHolderExistingModel);
                    await _repository.CardHolderRepository.SaveAsync();

                    UpdateCardholder(cardHolderExistingModel);
                }
                return APIResponseFactory.Success(ErrorMessages.DataSaved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the transaction.");
                return APIResponseFactory.Failure(HttpStatusCode.InternalServerError, "An error occurred while saving the transaction.");
            }
        }

        public async Task<APIResponse> Update(Guid id, TransactionRequest model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }

                var existingModel =
                    _repository.TransactionRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.RecordNotFound);
                }

                bool PreviousPaidStatus = existingModel.IsPaid ?? false;
                bool CurrentPaidStatus = model.IsPaid;

                Guid PreviousCardHolderId = existingModel.CardHolderId;
                Guid CurrentCardHolderId = model.CardHolderId;

                decimal PreviousAmount = existingModel.Amount;
                decimal CurrentAmount = model.Amount;
                decimal differenceAmount = (CurrentAmount - PreviousAmount);

                existingModel.Amount = model.Amount;
                existingModel.CategoryId = model.CategoryId;
                existingModel.SubCategoryId = model.SubCategoryId;
                existingModel.CardHolderId = model.CardHolderId;
                existingModel.Description = model.Description;
                existingModel.IsPaid = model.IsPaid;
                existingModel.TransactionDate = UtilityHelper.GetIndianTimeZoneDatetime(existingModel.TransactionDate);
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.TransactionRepository.Update(existingModel);
                await _repository.TransactionRepository.SaveAsync();

                //increas or dicreas amount in main balance

                if (PreviousPaidStatus == true && CurrentPaidStatus == true)
                {
                    var currentCardHolderDetail =
                     await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == CurrentCardHolderId && x.CreatedBy == currentUserId);

                    if (currentCardHolderDetail is null)
                    {
                        throw new Exception(ErrorMessages.CardHolderNotFound);
                    }

                    if (CurrentCardHolderId != PreviousCardHolderId)
                    {
                        var previousCardHolderDetail =
                         await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == PreviousCardHolderId && x.CreatedBy == currentUserId);

                        if (previousCardHolderDetail is null)
                        {
                            throw new Exception(ErrorMessages.CardHolderNotFound);
                        }

                        //add amount in previous cardholder with previous amount
                        previousCardHolderDetail.Balance += PreviousAmount;
                        _repository.CardHolderRepository.Update(previousCardHolderDetail);
                        await _repository.CardHolderRepository.SaveAsync();
                        UpdateCardholder(previousCardHolderDetail);


                        //remove current amount in current selected card holder
                        currentCardHolderDetail.Balance -= CurrentAmount;
                        _repository.CardHolderRepository.Update(currentCardHolderDetail);
                        await _repository.CardHolderRepository.SaveAsync();
                        UpdateCardholder(currentCardHolderDetail);

                    }
                    else
                    {
                        currentCardHolderDetail.Balance += differenceAmount;
                        _repository.CardHolderRepository.Update(currentCardHolderDetail);
                        await _repository.CardHolderRepository.SaveAsync();
                        UpdateCardholder(currentCardHolderDetail);

                    }
                }
                else if (PreviousPaidStatus == false && CurrentPaidStatus == true)
                {
                    var currentCardHolderDetail =
                     await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == CurrentCardHolderId && x.CreatedBy == currentUserId);
                    if (currentCardHolderDetail is null)
                    {
                        throw new Exception(ErrorMessages.CardHolderNotFound);
                    }
                    currentCardHolderDetail.Balance -= CurrentAmount;
                    _repository.CardHolderRepository.Update(currentCardHolderDetail);
                    await _repository.CardHolderRepository.SaveAsync();
                    UpdateCardholder(currentCardHolderDetail);


                }
                else if (PreviousPaidStatus == true && CurrentPaidStatus == false)
                {
                    var previousCardHolderDetail =
                         await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == PreviousCardHolderId && x.CreatedBy == currentUserId);

                    if (previousCardHolderDetail is null)
                    {
                        throw new Exception(ErrorMessages.CardHolderNotFound);
                    }
                    previousCardHolderDetail.Balance += PreviousAmount;
                    _repository.CardHolderRepository.Update(previousCardHolderDetail);
                    await _repository.CardHolderRepository.SaveAsync();
                    UpdateCardholder(previousCardHolderDetail);
                }


                _cacheService.Remove($"{CacheKeys.GetAllTransaction}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating transaction.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public async Task<APIResponse> Delete(Guid id)
        {
            var existingModel = _repository.TransactionRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
            if (existingModel is null)
            {
                throw new Exception(ErrorMessages.RecordNotFound);

            }

            existingModel.IsActive = false;
            existingModel.UpdatedBy = _repository.GetLoggedInUserId();
            existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

            _repository.TransactionRepository.Update(existingModel);
            await _repository.TransactionRepository.SaveAsync();
            _cacheService.Remove($"{CacheKeys.GetAllTransaction}_{currentUserId}");

            //increas  amount in main balance

            var cardHolderExistingModel = await _repository.CardHolderRepository.DataSet.FirstOrDefaultAsync(x => x.Id == existingModel.CardHolderId && x.CreatedBy == currentUserId);
            if (cardHolderExistingModel is null)
            {
                throw new Exception(ErrorMessages.CardHolderNotFound);
            }
            cardHolderExistingModel.Balance += existingModel.Amount;

            _repository.CardHolderRepository.Update(cardHolderExistingModel);
            await _repository.CardHolderRepository.SaveAsync();
            UpdateCardholder(cardHolderExistingModel);

            return APIResponseFactory.Success(ErrorMessages.DataDeleted);
        }

        private IList<Transaction>? GetCachedTransaction()
        {
            return _cacheService.Get<IList<Transaction>?>($"{CacheKeys.GetAllTransaction}_{currentUserId}");
        }

        private IList<CardHolder>? GetCachedCardHolder()
        {
            return _cacheService.Get<IList<CardHolder>?>($"{CacheKeys.GetAllCardHolder}_{currentUserId}");
        }

        public void UpdateCardholder(CardHolder updatedModel)
        {
            IList<CardHolder>? cachedData = GetCachedCardHolder();
            if (cachedData != null)
            {
                int index = cachedData.ToList().FindIndex(x => x.Id == updatedModel.Id && x.CreatedBy == currentUserId);
                if (index != -1)
                {
                    cachedData[index] = updatedModel;
                }
                else
                {
                    cachedData.Add(updatedModel);
                }
                _cacheService.Set($"{CacheKeys.GetAllCardHolder}_{currentUserId}", cachedData, null);
            }
        }

    }
}
