using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;

        public WalletService(
            ILogger<WalletService> logger,
            ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void AddWallet(Wallet wallet)
        {
            wallet.Balance = wallet.InitMoney;
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
        }

        public void DeleteWallet(int id)
        {
            var wallet = _context.Wallets.SingleOrDefault(x => x.WalletId == id);
            _context.Wallets.Remove(wallet);
            _context.SaveChanges();
        }

        public Wallet GetWallet(int id)
        {
            return _context.Wallets.Include(w => w.Possesions).ThenInclude(c => c.Stock).SingleOrDefault(x => x.WalletId == id);
        }

        public IEnumerable<Wallet> GetWallets(int userId)
        {
            return _context.Wallets.Where(x => x.OwnerId == userId).Include(w => w.Possesions).ThenInclude(c => c.Stock);
        }

        public void UpdateWallet(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            _context.SaveChanges();
        }
    }
}
