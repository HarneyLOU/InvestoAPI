using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure;
using InvestoAPI.Infrastructure.Services.StockProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{

    public class StockService : IStockService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationContext _context;
        static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

        public StockService(
            ILogger<StockService> logger,
            IMapper mapper,
            ApplicationContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        public Stock GetStockCurrent(string symbol)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Stock> GetStockHistory(string symbol, string from, string to, string interval = "d")
        {
            DateTime fromDate;
            if (!DateTime.TryParse(from, out fromDate)) fromDate = DateTime.Now.AddYears(-100);
            DateTime toDate;
            if (!DateTime.TryParse(to, out toDate)) toDate = DateTime.Now;

            int companyId = _context.Companies.Where(s => s.Symbol == symbol).Select(x => x.CompanyId).FirstOrDefault();
            return _context.Stocks.Where(
            s => s.CompanyId == companyId &&
            s.Date >= fromDate &&
            s.Date <= toDate);

        }
    }
}
