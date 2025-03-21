using Commons.Classes;
using Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Models.ViewModels;
using Models.ViewModels.Input;

namespace Application.Contracts
{
    public interface ICategoryService
    {
        Task<APIResponse<IList<Category>>> GetAll();
        Task<APIResponse<Category>> GetCategoryById(Guid id);
        Task<APIResponse<IList<Category>>> GetAllCategoryByType(int id);
        Task<APIResponse> SaveCategory(CategoryInput model);
        Task<APIResponse> UpdateCategory(Guid id, CategoryInput model);
        Task<APIResponse> DeleteCategory(Guid id);
        Task<APIResponse<SubCategory>> GetSubCategoryById(Guid id);
        Task<APIResponse<IList<SubCategory>>> GetSubCategoryByCategoryId(Guid id);
        Task<APIResponse> SaveSubCategory(SubCategoryInput model);
        Task<APIResponse> UpdateSubCategory(Guid id, SubCategoryInput model);
        Task<APIResponse> DeleteSubCategory(Guid id);
    }
}
