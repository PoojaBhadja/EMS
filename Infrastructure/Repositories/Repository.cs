using Domain;
using Models.Entities;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Http; 
using System.Security.Claims;
using Commons.Constants; 

namespace Infrastructure.Repositories
{ 
    public class Repository : IRepository
    {
        private IHttpContextAccessor _httpContextAccessor; 
        private ExpenceManagementSystemContext _repoContext; 
        public Repository(ExpenceManagementSystemContext repoContext, IHttpContextAccessor httpContextAccessor)
        {
            _repoContext = repoContext;
            _httpContextAccessor = httpContextAccessor; 
        }
 
        private ICardHolderRepository _CardHolderRepository;
        public ICardHolderRepository CardHolderRepository
        {
            get
            {
                if (_CardHolderRepository == null)
                {
                    _CardHolderRepository = new CardHolderRepository(_repoContext);
                }
                return _CardHolderRepository;
            }
        }
 
        private ICategoryRepository _CategoryRepository;
        public ICategoryRepository CategoryRepository
        {
            get
            {
                if (_CategoryRepository == null)
                {
                    _CategoryRepository = new CategoryRepository(_repoContext);
                }
                return _CategoryRepository;
            }
        }
 
        private IFixedAllocationRepository _FixedAllocationRepository;
        public IFixedAllocationRepository FixedAllocationRepository
        {
            get
            {
                if (_FixedAllocationRepository == null)
                {
                    _FixedAllocationRepository = new FixedAllocationRepository(_repoContext);
                }
                return _FixedAllocationRepository;
            }
        }
 
        private ISubCategoryRepository _SubCategoryRepository;
        public ISubCategoryRepository SubCategoryRepository
        {
            get
            {
                if (_SubCategoryRepository == null)
                {
                    _SubCategoryRepository = new SubCategoryRepository(_repoContext);
                }
                return _SubCategoryRepository;
            }
        }
 
        private ITransactionRepository _TransactionRepository;
        public ITransactionRepository TransactionRepository
        {
            get
            {
                if (_TransactionRepository == null)
                {
                    _TransactionRepository = new TransactionRepository(_repoContext);
                }
                return _TransactionRepository;
            }
        }
 
        private IUserRepository _UserRepository;
        public IUserRepository UserRepository
        {
            get
            {
                if (_UserRepository == null)
                {
                    _UserRepository = new UserRepository(_repoContext);
                }
                return _UserRepository;
            }
        }
        public Guid GetLoggedInUserId()
        {
            return Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirstValue(JwtCustomClaimNames.UserId));
        }
    }
}
