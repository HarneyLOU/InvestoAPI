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

        private static readonly HttpClient _client = new HttpClient();
        private static readonly string ApiKey = "6d8b92e71ff4054f10b30b35dee604ae";

        public FMProvider()
        {
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

        public async Task<FMNews[]> GetNews(string[] symbols)
        {
            string joinedSymbols = String.Join(",", symbols);
            var response = await _client.GetAsync($"https://financialmodelingprep.com/api/v3/stock_news?tickers={joinedSymbols}&limit=50&apikey={ApiKey}");
            var newsJson = await response.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<FMNews[]>(newsJson);
            return news.ToArray();
        }

        public async Task<FMSplit[]> GetSplits(DateTimeOffset from)
        {
            var fromString = from.ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"https://financialmodelingprep.com/api/v3/stock_split_calendar?from={fromString}&apikey={ApiKey}");
            var splitsJson = await response.Content.ReadAsStringAsync();
            var splits = JsonSerializer.Deserialize<FMSplit[]>(splitsJson);
            return splits.ToArray();
        }

        public async Task<FMQuote[]> GetHistoricalPrices(string symbol, DateTime from)
        {
            var fromString = from.ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"https://financialmodelingprep.com/api/v3/historical-price-full/{symbol}?from={fromString}&apikey={ApiKey}");
            var quoteJson = await response.Content.ReadAsStringAsync();
            var quotes = JsonSerializer.Deserialize<FMHistorical>(quoteJson);
            if (quotes.historical == null) return null;
            return quotes.historical.ToArray();
        }

        public string GetStockQuotes()
        {
            throw new NotImplementedException();
        }
    }

    public class FMNews
    {
        public string symbol { get; set; }
        public string publishedDate { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string site { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }

    public class FMSplit
    {
        public string date { get; set; }
        public string label { get; set; }
        public string symbol { get; set; }
        public float numerator { get; set; }
        public float denominator { get; set; }
    }

    public class FMHistorical
    {
        public string symbol { get; set; }
        public FMQuote[] historical { get; set; }
    }

    public class FMQuote
    {
        public string date { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }
        public double volume { get; set; }
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

        public static explicit operator CommonCompanyProfile(FMCompanyProfile com)
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
