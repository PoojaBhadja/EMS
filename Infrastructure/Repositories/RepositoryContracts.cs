using Domain;
using Infrastructure.Repositories.Base;
using Models.Entities;
using Infrastructure.DataAccess;


namespace Infrastructure.Repositories 
{
    public partial class CardHolderRepository : RepositoryBase<CardHolder>, ICardHolderRepository
    {
        public CardHolderRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
    public partial class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
    public partial class FixedAllocationRepository : RepositoryBase<FixedAllocation>, IFixedAllocationRepository
    {
        public FixedAllocationRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
    public partial class SubCategoryRepository : RepositoryBase<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
    public partial class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
    public partial class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ExpenceManagementSystemContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}

