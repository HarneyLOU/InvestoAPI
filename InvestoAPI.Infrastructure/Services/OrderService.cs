using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Enums;
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
        private readonly IWalletStateService _walletStateService;
        private readonly ITransactionService _transactionService;

        public OrderService(
            ILogger<OrderService> logger,
            ApplicationContext context,
            IWalletStateService walletStateService,
            ITransactionService transactionService)
        {
            _logger = logger;
            _context = context;
            _walletStateService = walletStateService;
            _transactionService = transactionService;
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(setStatus(order));
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
                .Where(w => w.WalletId == walletId).OrderByDescending(d => d.Created);
        }

        public Order GetOrder(int id)
        {
            return _context.Orders.Include(w => w.Wallet).SingleOrDefault(x => x.OrderId == id);
        }

        public IEnumerable<Order> GetActiveOrders()
        {
            return _context.Orders.Include(w => w.Wallet)
                .Where(x => x.Active == true && x.ActivationDate <= DateTime.Now)
                .OrderBy(d => d.Created);
        }

        public Order CancelOrder(Order order)
        {
            if(order.StatusEnum == OrderStatusEnum.Pending)
            {
                order.Status = "Cancelled - on request";
                order.StatusEnum = OrderStatusEnum.Cancelled;
                order.Active = false;
                UpdateOrder(order);
            }
            return order;
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        private Order setStatus(Order order)
        {
            order.Created = DateTime.Now;
            order.Status = "Pending";
            order.StatusEnum = OrderStatusEnum.Pending;
            order.Active = true;

            if (!order.ExpiryDate.HasValue) order.ExpiryDate = order.Created.Date.AddDays(2);
            else
            {
                order.ExpiryDate = order.ExpiryDate.Value;
                if (order.ExpiryDate < DateTime.Now)
                {
                    order.Status = "Cancelled - expired";
                    order.StatusEnum = OrderStatusEnum.Cancelled;
                    order.Active = false;
                }
            }
            if (!order.ActivationDate.HasValue) order.ActivationDate = order.Created.Date;
            else order.ActivationDate = order.ActivationDate.Value;
            return order;
        } 

        async public Task<Order> ConsumeTransaction(Transaction transaction)
        {
            await using var transactionContext = await _context.Database.BeginTransactionAsync();
            try
            {
                var newOrder = _walletStateService.UpsertWalletState(transaction);
                UpdateOrder(newOrder);
                if (newOrder.StatusEnum == OrderStatusEnum.Success 
                    || newOrder.StatusEnum == OrderStatusEnum.Partially) _transactionService.AddTransaction(transaction);
                await transactionContext.CommitAsync();
                return newOrder;
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR - {ex.Message}");
            }
            return null;
        }
    }
}
