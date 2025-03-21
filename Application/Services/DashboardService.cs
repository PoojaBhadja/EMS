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
    public class DashboardService : IDashboardService
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Guid currentUserId;

        public DashboardService(IRepository repository, IMapper mapper, ILogger<TransactionService> logger, ICacheService cacheService)
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

        public async Task<APIResponse<DashboardVm>> GetDashboardSummary(FilterDataVm filterDataVm)
        {
            try
            {
                IList<Transaction>? cachedData = GetCachedTransaction();

                DashboardVm? dbset = cachedData?
                            .Where(x => x.IsActive == true)
                            .GroupBy(t => 1)
                            .Select(g => new DashboardVm
                            {
                                TotalIncome = g.Where(t => t.TransactionType == (int)CategoryType.Income && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                TotalExpence = g.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                TotalInvestment = g.Where(t => t.TransactionType == (int)CategoryType.Investment && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                ExpenceBreakdowns = g.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).GroupBy(x => x.CategoryId)
                                .Select(a => new ExpenceBrekdownVm
                                {
                                    Amount = a.Sum(am => am.Amount),
                                    Name = a.FirstOrDefault()?.Category?.Name ?? "unkknow"
                                }).ToList(),
                                IncomeBreakdowns = g.Where(t => t.TransactionType == (int)CategoryType.Income && t.IsPaid == true && t.CreatedBy == currentUserId).GroupBy(x => x.CategoryId)
                                .Select(a => new IncomeBrekdownVm
                                {
                                    Amount = a.Sum(am => am.Amount),
                                    Name = a.FirstOrDefault()?.Category?.Name ?? "unkknow"
                                }).ToList(),
                                Transactions = g.OrderByDescending(x => x.CreatedDate).Take(5).ToList(),
                                CashFlowVm = g.GroupBy(t => t.TransactionDate.Month) // Group by Month
                                            .Select(m => new CashFlowVm
                                            {
                                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Key),
                                                IncomeAmount = m.Where(t => t.TransactionType == (int)CategoryType.Income && t.CreatedBy == currentUserId)
                                                               .Sum(t => (decimal?)t.Amount) ?? 0,
                                                ExpenceAmount = m.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.CreatedBy == currentUserId)
                                                                .Sum(t => (decimal?)t.Amount) ?? 0,
                                                InvestedAmount = m.Where(t => t.TransactionType == (int)CategoryType.Investment && t.IsPaid == true && t.CreatedBy == currentUserId)
                                                                .Sum(t => (decimal?)t.Amount) ?? 0,

                                            }).OrderBy(x => x.Month).ToList()
                            })
                            .FirstOrDefault() ??
                    await _repository.TransactionRepository.DataSet
                            .Where(x => x.IsActive == true && x.CreatedBy == currentUserId)
                           .AsNoTracking()
                            .Include(x => x.Category)
                            .Include(x => x.SubCategory)
                            .Include(x => x.CardHolder)
                            .GroupBy(t => 1)
                            .Select(g => new DashboardVm
                            {
                                TotalIncome = g.Where(t => t.TransactionType == (int)CategoryType.Income && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                TotalExpence = g.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                TotalInvestment = g.Where(t => t.TransactionType == (int)CategoryType.Investment && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).Sum(t => (decimal?)t.Amount) ?? 0,
                                ExpenceBreakdowns = g.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.TransactionDate.Month == filterDataVm.Month && t.CreatedBy == currentUserId).GroupBy(x => x.CategoryId)
                                .Select(a => new ExpenceBrekdownVm
                                {
                                    Amount = a.Sum(am => am.Amount),
                                    Name = (a.FirstOrDefault() ?? new Transaction()).Category.Name
                                }).ToList(),
                                IncomeBreakdowns = g.Where(t => t.TransactionType == (int)CategoryType.Income && t.IsPaid == true && t.CreatedBy == currentUserId).GroupBy(x => x.CategoryId)
                                .Select(a => new IncomeBrekdownVm
                                {
                                    Amount = a.Sum(am => am.Amount),
                                    Name = (a.FirstOrDefault() ?? new Transaction()).Category.Name
                                }).ToList(),
                                Transactions = g.OrderByDescending(x => x.CreatedDate).Take(5).ToList(),
                                CashFlowVm = g.GroupBy(t => t.TransactionDate.Month) // Group by Month
                                            .Select(m => new CashFlowVm
                                            {
                                                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Key),
                                                IncomeAmount = m.Where(t => t.TransactionType == (int)CategoryType.Income && t.CreatedBy == currentUserId)
                                                               .Sum(t => (decimal?)t.Amount) ?? 0,
                                                ExpenceAmount = m.Where(t => t.TransactionType == (int)CategoryType.Expence && t.IsPaid == true && t.CreatedBy == currentUserId)
                                                                .Sum(t => (decimal?)t.Amount) ?? 0,
                                                InvestedAmount = m.Where(t => t.TransactionType == (int)CategoryType.Investment && t.IsPaid == true && t.CreatedBy == currentUserId)
                                                                .Sum(t => (decimal?)t.Amount) ?? 0,
                                            }).ToList()
                            }).FirstOrDefaultAsync();

                return dbset != null
                      ? APIResponseFactory.Success(dbset)
                      : APIResponseFactory.Failure<DashboardVm>(HttpStatusCode.NoContent, "No dashboard data available.");
            }
            catch (Exception)
            {
                throw;
            }

        }

        private IList<Transaction>? GetCachedTransaction()
        {
            return _cacheService.Get<IList<Transaction>?>($"{CacheKeys.GetAllTransaction}_{currentUserId}");
        }
    }
}
