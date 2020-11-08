using InvestoAPI.Infrastructure.Services.StockProviders.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services.StockProviders
{
    public class FinnhubProvider
    {

        private readonly HttpClient _client;
        private readonly string ApiKey;

        public FinnhubProvider(HttpClient client)
        {
            _client = client;
            ApiKey = "bpv6tnnrh5rabkt31r10";
        }

        public async Task<CommonCompanyProfile[]> GetCompanyProfiles(string[] symbols)
        {
            Task<CommonCompanyProfile>[] datas = new Task<CommonCompanyProfile>[symbols.Length];
            int i = 0;
            foreach(var symbol in symbols)
            {
                datas[i++] = CallForCompanyProfile(symbol);
            }
            var companies = await Task.WhenAll(datas);
            return companies;
        }

        private async Task<CommonCompanyProfile> CallForCompanyProfile(string symbol)
        {
            var response = await _client.GetAsync($"https://finnhub.io/api/v1/stock/profile2?symbol={symbol}&token={ApiKey}");
            var companyJson = await response.Content.ReadAsStringAsync();
            var finnCompany = JsonSerializer.Deserialize<FinnhubCompanyProfile>(companyJson);
            return (CommonCompanyProfile)finnCompany;
        }

        public string GetStockQuotes()
        {
            throw new NotImplementedException();
        }
    }

    public class FinnhubCompanyProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("ipo")]
        public DateTime IpoDate { get; set; }

        //public string Image { get; set; }

        //public string Url { get; set; }

        //public string Exchange { get; set; }

        //public string Industry { get; set; }

        //public string Currency { get; set; }

        //[Column(TypeName = "decimal(18,4)")]
        //public decimal MarketCap { get; set; }

        //public string Description { get; set; }

        public static explicit operator CommonCompanyProfile(FinnhubCompanyProfile com)
        {
            CommonCompanyProfile commoncom = new CommonCompanyProfile()
            {
                Name = com.Name,
                Symbol = com.Symbol,
                IpoDate = com.IpoDate
            };
            return commoncom;
        }
    }
}
