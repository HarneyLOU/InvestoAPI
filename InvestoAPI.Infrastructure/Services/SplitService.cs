using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure.Services.StockProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{
    public class SplitService : ISplitService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;
        private readonly ICompanyService _companyService;
        private readonly IStockService _stockService;

        public SplitService(
            ILogger<SplitService> logger,
            ApplicationContext context,
            ICompanyService companyService,
            IStockService stockService)
        {
            _logger = logger;
            _context = context;
            _companyService = companyService;
            _stockService = stockService;
        }

        public IEnumerable<Split> GetAll()
        {
            return _context.Split;
        }

        public async Task Split(string[] symbols)
        {
            var lastSplitDate = _context.Split.Count() > 0 ? _context.Split.Max(s => s.Date).AddDays(1) : DateTimeOffset.Now.Date;

            var dataProvider = new FMProvider();
            try
            {
                var fmSplits = await dataProvider.GetSplits(lastSplitDate);
                List<Split> newSplits = new List<Split>();

                foreach (var split in fmSplits.Where(s => symbols.Contains(s.symbol)))
                {
                    newSplits.Add(new Split()
                    {
                        StockId = _companyService.Get(split.symbol).StockId,
                        Date = DateTime.Parse(split.date),
                        Denominator = split.denominator,
                        Numerator = split.numerator
                    });
                }
                _context.Split.AddRange(newSplits);
                _context.SaveChanges();

                foreach (var split in newSplits)
                {
                    await _stockService.Split(split.StockId, split.Numerator / split.Denominator);
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("ERROR " + ex.Message);
            }

           
        }
    }
}
