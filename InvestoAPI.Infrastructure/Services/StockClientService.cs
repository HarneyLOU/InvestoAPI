using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure;
using InvestoAPI.Infrastructure.Services.StockProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{

    public class StockClientService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

        public StockClientService(
            ILogger<StockService> logger,
            IServiceScopeFactory scopeFactory,
            HttpClient client,
            IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _client = client;
            _mapper = mapper;
        }

        //public async Task<string[]> GetStocksList()
        //{
        //    var stockProvider = new FMProvider(_client);
        //    return await stockProvider.GetDowJonesSymbols();
        //}

        //public async Task AddCompanyProfiles()
        //{
        //    using var scope = _scopeFactory.CreateScope();
        //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        //    var stockProvider = new FMProvider(_client);

        //    var companiesFromService = await stockProvider.GetCompanyProfiles(await stockProvider.GetDowJonesSymbols());
        //    var companies = _mapper.Map<Company[]>(companiesFromService);

        //    DateTime now = DateTime.Now;
        //    foreach (var company in companies)
        //    {
        //        company.UpdateTime = now;
        //    }
        //    await dbContext.AddRangeAsync(companies);
        //    await dbContext.SaveChangesAsync();
        //}

        //public Task SaveRealTimeStockQuotes()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
