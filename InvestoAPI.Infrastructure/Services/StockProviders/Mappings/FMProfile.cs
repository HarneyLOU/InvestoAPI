using AutoMapper;
using InvestoAPI.Core.Entities;

namespace InvestoAPI.Infrastructure.Services.StockProviders.Mappings
{
    public class FMProfile : Profile
    {
        public FMProfile()
        {
            CreateMap<CommonCompanyProfile, Company>();
        }
    }
}
