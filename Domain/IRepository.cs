 

namespace Domain
{ 
    public interface IRepository 
    {
        ICardHolderRepository CardHolderRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IFixedAllocationRepository FixedAllocationRepository { get; }
        ISubCategoryRepository SubCategoryRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IUserRepository UserRepository { get; }
        Guid GetLoggedInUserId();
    }
}
