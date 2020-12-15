using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.Profiles
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<RegisterViewModel, User>();
            CreateMap<UpdateViewModel, User>();
            CreateMap<OrderViewModel, Order>()
                .ReverseMap()
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Company.Symbol));
            CreateMap<WalletViewModel, Wallet>()
                .ReverseMap();
            CreateMap<WalletStateViewModel, WalletState>()
                .ReverseMap()
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Stock.Symbol));
            CreateMap<TransactionViewModel, Transaction>().ReverseMap();
            CreateMap<TeamViewModel, Team>().ReverseMap();
        }
    }
}
