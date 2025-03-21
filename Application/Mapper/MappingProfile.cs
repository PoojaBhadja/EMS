using AutoMapper;
using Models.Entities;
using Models.ViewModels;
using Models.ViewModels.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryInput>();
            CreateMap<CategoryInput, Category>();

            CreateMap<SubCategory, SubCategoryInput>();
            CreateMap<SubCategoryInput, SubCategory>();

            CreateMap<AccountDetailInput, AccountDetail>();
            CreateProjection<AccountDetail, AccountDetailVm>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Bank.BankName + " - " + src.CardHolderName))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank.BankName));

            CreateMap<AccountDetailVm, AccountDetail>();

            CreateMap<Bank, BankVm>();

            CreateMap<TransactionRequest, Transaction>();
            //CreateMap<TransactionRequest, TransactionRequest>();
            //CreateMap<ExpenceIncome, ExpenceIncomeVm>()
            //    .ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.AccountDetail.Bank.BankName + " - " + src.AccountDetail.CardHolderName))
            //    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name));
            //CreateMap<ExpenceIncomeVm, ExpenceIncome>();


            //CreateMap<ExpenceIncome, TransactionDetailsVm>()
            //     .ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.AccountDetail.Bank.BankName + " - " + src.AccountDetail.CardHolderName))
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            //CreateMap<ExpenceIncome, DifferTransactionDetails>()
            // .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            // .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));

            CreateMap<CardHolder, CardHolderInput>();
            CreateMap<CardHolderInput, CardHolder>();


            CreateMap<FixedAllocation, FixedAllocationRequest>();
            CreateMap<FixedAllocationRequest, FixedAllocation>();
        }
    }
}
