using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class TransactionService: ITransactionService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;

        public TransactionService(
            ILogger<TransactionService> logger,
            ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void AddTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }
    }
}
