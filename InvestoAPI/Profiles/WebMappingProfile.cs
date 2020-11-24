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
            CreateMap<WalletViewModel, Wallet>().ReverseMap();
        }
    }
}
