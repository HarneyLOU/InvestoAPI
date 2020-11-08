using InvestoAPI.Infrastructure.Services.StockProviders;
using InvestoAPI.Infrastructure.Services.StockProviders.Mappings;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services.StockProviders
{
    public class FMProvider : IDataProvider
    {

        private readonly HttpClient _client;
        private readonly string ApiKey;

        public FMProvider(HttpClient client)
        {
            _client = client;
            ApiKey = "6d8b92e71ff4054f10b30b35dee604ae";
        }

        public async Task<CommonCompanyProfile[]> GetCompanyProfiles(string[] symbols)
        {
            var joinedSymbols = String.Join(",", symbols);
            var response = await _client.GetAsync($"https://financialmodelingprep.com/api/v3/profile/{joinedSymbols}?apikey={ApiKey}");
            var companyJson = await response.Content.ReadAsStringAsync();
            var fmCompanies = JsonSerializer.Deserialize<FMCompanyProfile[]>(companyJson);
            var companies = fmCompanies.Select(c => (CommonCompanyProfile)c).ToArray();
            return companies;
        }

        public async Task<string[]> GetDowJonesSymbols()
        {
            var response = await _client.GetAsync($"https://financialmodelingprep.com/api/v3/dowjones_constituent?apikey={ApiKey}");
            var symbolsJson = await response.Content.ReadAsStringAsync();
            var fmSymbols = JsonSerializer.Deserialize<FMCompanySymbol[]>(symbolsJson);
            var symbols = fmSymbols.Select(s => s.Symbol);
            return symbols.ToArray();

        }

        public string GetStockQuotes()
        {
            throw new NotImplementedException();
        }
    }

    public class FMCompanySymbol
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }
    }

    public class FMCompanyProfile
    {
        [JsonPropertyName("companyName")]
        public string Name { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("ipoDate")]
        public DateTime IpoDate { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("website")]
        public string Url { get; set; }

        [JsonPropertyName("exchangeShortName")]
        public string Exchange { get; set; }

        [JsonPropertyName("industry")]
        public string Industry { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("mktCap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        public static explicit operator CommonCompanyProfile (FMCompanyProfile com)
        {
            CommonCompanyProfile commoncom = new CommonCompanyProfile()
            {
                Name = com.Name,
                Symbol = com.Symbol,
                IpoDate = com.IpoDate,
                Image = com.Image,
                Url = com.Url,
                Exchange = com.Exchange,
                Industry = com.Industry,
                Currency = com.Currency,
                MarketCap = com.MarketCap,
                Description = com.Description
            };
            return commoncom;
        }
    }
}
