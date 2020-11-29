using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvestoAPI.Infrastructure.Services
{
    public class OrderService: IOrderService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;
        private readonly OrderQueueService _orderQueueService;

        public OrderService(
            ILogger<OrderService> logger,
            ApplicationContext context,
            OrderQueueService orderQueueService)
        {
            _logger = logger;
            _context = context;
            _orderQueueService = orderQueueService;
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public void DeleteOrder(int id)
        {
            var order = _context.Orders.SingleOrDefault(x => x.OrderId == id);
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

        public IEnumerable<Order> GetOrdersFromWallet(int walletId)
        {
            return _context.Orders
                .Include(t => t.Transactions)
                .Include(c => c.Company)
                .Where(w => w.WalletId == walletId);
        }

        public Order GetOrder(int id)
        {
            return _context.Orders.Include(w => w.Wallet).SingleOrDefault(x => x.OrderId == id);
        }

        public IEnumerable<Order> GetActiveOrders()
        {
            return _context.Orders.Include(w => w.Wallet).Where(x => x.Active == true).OrderBy(d => d.Created);
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }
    }
}
