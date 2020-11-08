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
            return _context.Companies;
        }

        public IEnumerable<Company> GetDowJones()
        {
            return _context.Companies.Where(c => c.MarketIndex == "DJIA");
        }
    }
}
