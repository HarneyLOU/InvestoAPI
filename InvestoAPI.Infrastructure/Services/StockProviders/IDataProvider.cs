using InvestoAPI.Infrastructure.Services.StockProviders.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services.StockProviders
{
    public interface IDataProvider
    {
        public Task<CommonCompanyProfile[]> GetCompanyProfiles(string[] symbol);
        public Task<string[]> GetDowJonesSymbols();
    }
}
