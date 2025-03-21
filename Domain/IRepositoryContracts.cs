using Domain.Base;
using Models.Entities;

namespace Domain
{ 
    public partial interface ICardHolderRepository : IRepositoryBase<CardHolder>
    {
    }
    public partial interface ICategoryRepository : IRepositoryBase<Category>
    {
    }
    public partial interface IFixedAllocationRepository : IRepositoryBase<FixedAllocation>
    {
    }
    public partial interface ISubCategoryRepository : IRepositoryBase<SubCategory>
    {
    }
    public partial interface ITransactionRepository : IRepositoryBase<Transaction>
    {
    }
    public partial interface IUserRepository : IRepositoryBase<User>
    {
    }
}
