using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services.StockProviders.Mappings
{
    public class CommonCompanyProfile
    {
        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public DateTime IpoDate { get; set; }

        public string Image { get; set; }

        public string Url { get; set; }

        public string Exchange { get; set; }

        public string Industry { get; set; }

        public string Currency { get; set; }

        public decimal MarketCap { get; set; }

        public string Description { get; set; }
    }
}
