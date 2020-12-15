using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    class CompanyService : ICompanyService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;

        public CompanyService(
            ILogger<CompanyService> logger,
            ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IEnumerable<Company> GetAll()
        {
            return _context.Companies.Distinct(); ;
        }

        public Company Get(int id)
        {
            return _context.Companies.Where(c => c.StockId == id).FirstOrDefault();
        }

        public Company Get(string symbol)
        {
            return _context.Companies.Where(c => c.Symbol == symbol).FirstOrDefault();
        }

        public string GetImage(string symbol)
        {
            return _context.Companies.Where(c => c.Symbol == symbol).Select(c => c.Image).FirstOrDefault();
        }

        public IEnumerable<Company> GetDowJones()
        {
            return _context.Companies.Where(c => c.MarketIndex == "DJIA");
        }
    }
}
