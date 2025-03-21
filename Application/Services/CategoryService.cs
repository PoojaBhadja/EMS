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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Guid currentUserId;

        public CategoryService(IRepository repository, IMapper mapper, ILogger<CategoryService> logger, ICacheService cacheService)
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

        #region Category
        public async Task<APIResponse<IList<Category>>> GetAll()
        {
            try
            {
                IList<Category>? cachedData = GetCachedCategory();

                var dbset = cachedData?.Where(x => x.IsActive == true)
                   .OrderByDescending(x => x.CreatedDate).ToList() ??
                    await _repository.CategoryRepository.DataSet
                    .Include(x => x.SubCategories)
                    .Where(x => x.IsActive == true
                    && x.CreatedBy == currentUserId)
                    .AsNoTracking()
                   .OrderByDescending(x => x.CreatedDate).ToListAsync();

                if (cachedData == null)
                {
                    _cacheService.Set($"{CacheKeys.GetAllCategory}_{currentUserId}", dbset);
                }

                return dbset.Any()
                       ? APIResponseFactory.Success<IList<Category>>(dbset)
                       : APIResponseFactory.Failure<IList<Category>>(HttpStatusCode.NoContent, "No active category found.");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<APIResponse<Category>> GetCategoryById(Guid id)
        {
            try
            {
                IList<Category>? cachedData = GetCachedCategory();

                var dbset = cachedData?.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId) ??
                    await _repository.CategoryRepository.DataSet.FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == currentUserId);
                return dbset != null
                      ? APIResponseFactory.Success(dbset)
                      : APIResponseFactory.Failure<Category>(HttpStatusCode.NotFound, "Category not found.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception occurred while fetching catgory by ID.. Message :{ex.Message}");
                throw;
            }
        }

        public async Task<APIResponse<IList<Category>>> GetAllCategoryByType(int id)
        {
            try
            {
                IList<Category>? cachedData = GetCachedCategory();

                var dbset = cachedData?.Where(x => x.CategoryType == id && x.CreatedBy == currentUserId).ToList() ??
                    await _repository.CategoryRepository.DataSet.Where(x => x.CategoryType == id && x.CreatedBy == currentUserId).ToListAsync();
                return dbset != null
                      ? APIResponseFactory.Success<IList<Category>>(dbset)
                      : APIResponseFactory.Failure<IList<Category>>(HttpStatusCode.NotFound, "Category not found.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception occurred while fetching catgory by type.. Message :{ex.Message}");
                throw;
            }
        }

        public async Task<APIResponse> SaveCategory(CategoryInput model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }

                IList<Category>? cachedData = GetCachedCategory();
                bool isDuplicate = false;

                if (cachedData != null && cachedData.Any())
                {
                    isDuplicate = cachedData.Any(x => x.Name.ToLower() == model.Name.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDuplicate = _repository.CategoryRepository.DataSet.Any(x => x.Name.ToLower() == model.Name.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDuplicate)
                {
                    throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
                }

                var dbModel = _mapper.Map<Category>(model);

                dbModel.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();
                dbModel.CreatedBy = _repository.GetLoggedInUserId();

                _repository.CategoryRepository.Create(dbModel);
                await _repository.CategoryRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCategory}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataSaved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the Category.");
                return APIResponseFactory.Failure(HttpStatusCode.InternalServerError, "An error occurred while saving the Category.");
            }

        }

        public async Task<APIResponse> UpdateCategory(Guid id, CategoryInput model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }

                IList<Category>? cachedData = GetCachedCategory();

                var existingModel = cachedData?.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId) ??
                    _repository.CategoryRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.DataNotFound);
                }

                bool isDuplicate = false;

                if (cachedData != null && cachedData.Any())
                {
                    isDuplicate = cachedData.Any(x => x.Name.ToLower() == model.Name.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDuplicate = _repository.CategoryRepository.DataSet.Any(x => x.Name.ToLower() == model.Name.ToLower() && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDuplicate)
                {
                    throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
                }

                existingModel.Name = model.Name;
                existingModel.CategoryType = model.CategoryType;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.CategoryRepository.Update(existingModel);
                await _repository.CategoryRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCategory}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Category.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public async Task<APIResponse> DeleteCategory(Guid id)
        {
            try
            {
                IList<Category>? cachedData = GetCachedCategory();

                var existingModel = cachedData?.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId) ??
                    _repository.CategoryRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.RecordNotFound);
                }

                existingModel.IsActive = false;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.CategoryRepository.Update(existingModel);
                await _repository.CategoryRepository.SaveAsync();

                _cacheService.Remove($"{CacheKeys.GetAllCategory}_{currentUserId}");

                return APIResponseFactory.Success(ErrorMessages.DataDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Category with ID: {Id}", id);
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion

        #region Sub Category
        public async Task<APIResponse<SubCategory>> GetSubCategoryById(Guid id)
        {
            try
            {
                IList<SubCategory>? cachedData = GetCachedSubCategory();

                var dbset = cachedData?.FirstOrDefault(x => x.Id == id && x.IsActive == true && x.CreatedBy == currentUserId) ??
                    await _repository.SubCategoryRepository.DataSet.Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true && x.CreatedBy == currentUserId);

                return dbset != null
                      ? APIResponseFactory.Success(dbset)
                      : APIResponseFactory.Failure<SubCategory>(HttpStatusCode.NotFound, "Sub-Category not found.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception occurred while fetching sub-catgory by ID.. Message :{ex.Message}");
                throw;
            }
        }

        public async Task<APIResponse<IList<SubCategory>>> GetSubCategoryByCategoryId(Guid id)
        {
            try
            {
                IList<SubCategory>? cachedData = GetCachedSubCategory();

                var dbset = cachedData?.Where(x => x.CategoryId == id && x.IsActive == true && x.CreatedBy == currentUserId).ToList() ??
                    await _repository.SubCategoryRepository.DataSet
                    .Where(x => x.CategoryId == id && x.IsActive == true && x.CreatedBy == currentUserId).ToListAsync();
                return dbset != null
                      ? APIResponseFactory.Success<IList<SubCategory>>(dbset)
                      : APIResponseFactory.Failure<IList<SubCategory>>(HttpStatusCode.NotFound, "Sub-Category not found.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Exception occurred while fetching sub-catgory by category ID.. Message :{ex.Message}");
                throw;
            }
        }

        public async Task<APIResponse> SaveSubCategory(SubCategoryInput model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }
                bool isDublicate = false;
                //improve next time  //code for checking subcategory is not available withing transaction type
                //IList<SubCategory>? cachedData = GetCachedSubCategory();
                //if (cachedData != null && cachedData.Any())
                //{

                //    isDublicate = cachedData.Any(x => x.Name.ToLower() == model.Name.ToLower() && x.IsActive == true);

                //}
                //else
                //{

                isDublicate = _repository.SubCategoryRepository.DataSet.Any(x => x.Name.ToLower() == model.Name.ToLower()
                && x.IsActive == true && x.Category.CategoryType == model.SelectedTransactionType && x.CreatedBy == currentUserId);
                //}

                if (isDublicate)
                {
                    throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
                }

                var dbModel = _mapper.Map<SubCategory>(model);

                dbModel.CreatedDate = UtilityHelper.GetIndianTimeZoneDatetime();
                dbModel.CreatedBy = _repository.GetLoggedInUserId();

                _repository.SubCategoryRepository.Create(dbModel);
                await _repository.SubCategoryRepository.SaveAsync();

                //refresh catch
                UpdateSubCategoryCatch();

                return APIResponseFactory.Success(ErrorMessages.DataSaved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the Sub-Category.");
                return APIResponseFactory.Failure(HttpStatusCode.InternalServerError, "An error occurred while saving the Sub-Category.");
            }

        }

        public async Task<APIResponse> UpdateSubCategory(Guid id, SubCategoryInput model)
        {
            try
            {
                if (model == null)
                {
                    throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
                }
                IList<SubCategory>? cachedData = GetCachedSubCategory();

                var existingModel = cachedData?.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId) ??
                    _repository.SubCategoryRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.DataNotFound);
                }
                bool isDublicate = false;
                if (cachedData != null && cachedData.Any())
                {
                    isDublicate = cachedData.Any(
                    x => x.Name.ToLower() == model.Name.ToLower()
                && x.Id != id && x.IsActive == true && x.CreatedBy == currentUserId);
                }
                else
                {
                    isDublicate = _repository.SubCategoryRepository.DataSet.Any(
                    x => x.Name.ToLower() == model.Name.ToLower()
                && x.Id != id && x.IsActive == true && x.CreatedBy == currentUserId);
                }

                if (isDublicate)
                {
                    throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
                }

                existingModel.Name = model.Name;
                existingModel.CategoryId = model.CategoryId;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.SubCategoryRepository.Update(existingModel);
                await _repository.SubCategoryRepository.SaveAsync();

                UpdateSubCategoryCatch();

                return APIResponseFactory.Success(ErrorMessages.DataUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Sub-Category.");
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public async Task<APIResponse> DeleteSubCategory(Guid id)
        {
            try
            {
                IList<SubCategory>? cachedData = GetCachedSubCategory();

                var existingModel = cachedData?.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId) ??
                    _repository.SubCategoryRepository.DataSet.FirstOrDefault(x => x.Id == id && x.CreatedBy == currentUserId);
                if (existingModel is null)
                {
                    throw new Exception(ErrorMessages.RecordNotFound);
                }

                existingModel.IsActive = false;
                existingModel.UpdatedBy = _repository.GetLoggedInUserId();
                existingModel.UpdatedDate = UtilityHelper.GetIndianTimeZoneDatetime();

                _repository.SubCategoryRepository.Delete(existingModel);
                await _repository.CategoryRepository.SaveAsync();

                UpdateSubCategoryCatch();

                return APIResponseFactory.Success(ErrorMessages.DataDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Sub-Category with ID: {Id}", id);
                return APIResponseFactory.Failure(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion

        #region Private method

        private IList<Category>? GetCachedCategory()
        {
            return _cacheService.Get<IList<Category>?>($"{CacheKeys.GetAllCategory}_{currentUserId}");
        }

        private IList<SubCategory>? GetCachedSubCategory()
        {
            return _cacheService.Get<IList<SubCategory>?>($"{CacheKeys.GetAllSubCategory}_{currentUserId}");
        }

        public void UpdateSubCategoryCatch()
        {
            _cacheService.Remove($"{CacheKeys.GetAllSubCategory}_{currentUserId}");
            _cacheService.Remove($"{CacheKeys.GetAllCategory}_{currentUserId}");
            var data = _repository.SubCategoryRepository.DataSet
                .Where(x => x.IsActive == true)
                .AsNoTracking()
                .ToList();
            _cacheService.Set($"{CacheKeys.GetAllSubCategory}_{currentUserId}", data);
        }
        #endregion
    }
}
