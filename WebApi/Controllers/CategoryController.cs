using Application.Contracts;
using Application.Services;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModels;
using Models.ViewModels.Input;

namespace WebApi.Controllers
{
    [ApiVersion("1.0")]
    public class CategoryController : AutorizeController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _categoryService.GetAll());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            return Ok(await _categoryService.GetCategoryById(id));
        }

        [HttpGet("GetAllCategoryByType/{type:int}")]
        public async Task<IActionResult> GetCategoryByType(int type)
        {
            return Ok(await _categoryService.GetAllCategoryByType(type));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CategoryInput category)
        {
            return Ok(await _categoryService.SaveCategory(category));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, CategoryInput category)
        {
            return Ok(await _categoryService.UpdateCategory(id, category));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return Ok(await _categoryService.DeleteCategory(id));
        }


        [HttpGet("GetSubCategoryById/{id:guid}")]
        public async Task<IActionResult> GetSubCategoryById(Guid id)
        {
            return Ok(await _categoryService.GetSubCategoryById(id));
        }

        [HttpGet("GetSubCategoryByCategoryId/{id:guid}")]
        public async Task<IActionResult> GetSubCategoryByCategoryId(Guid id)
        {
            return Ok(await _categoryService.GetSubCategoryByCategoryId(id));
        }

        [HttpPost("SaveSubCategory")]
        public async Task<IActionResult> SaveSubCategory(SubCategoryInput model)
        {
            return Ok(await _categoryService.SaveSubCategory(model));
        }

        [HttpPut("UpdateSubCategory/{id:guid}")]
        public async Task<IActionResult> UpdateSubCategory(Guid id, SubCategoryInput model)
        {
            return Ok(await _categoryService.UpdateSubCategory(id, model));
        }

        [HttpDelete("DeleteSubCategory/{id:guid}")]
        public async Task<IActionResult> DeleteSubCategory([FromRoute] Guid id)
        {
            return Ok(await _categoryService.DeleteSubCategory(id));
        }

    }
}
