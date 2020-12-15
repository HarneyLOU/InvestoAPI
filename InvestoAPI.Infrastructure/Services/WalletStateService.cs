using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Enums;
using InvestoAPI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class WalletStateService : IWalletStateService
    {
        private readonly ApplicationContext _context;

        public WalletStateService(
            ApplicationContext context)
        {
            _context = context;
        }

        public Order UpsertWalletState(Transaction transaction)
        {
            var order = transaction.Order;
            var wallet = order.Wallet;

            if(order.ExpiryDate <= DateTime.Now)
            {
                order.Status = "Cancelled - expired";
                order.StatusEnum = OrderStatusEnum.Cancelled;
                order.Active = false;
                return order;
            }
            if(wallet == null)
            {
                order.Status = "Cancelled - wallet not exists";
                order.StatusEnum = OrderStatusEnum.Cancelled;
                order.Active = false;
                return order;
            }
            if(order.Buy == true && wallet.Balance - transaction.Price < 0)
            {
                order.Status = "Cancelled - not funds";
                order.StatusEnum = OrderStatusEnum.Cancelled;
                order.Active = false;
                return order;
            }
            var walletState = _context.WalletsState.FirstOrDefault(w => w.WalletId == order.WalletId && w.StockId == order.StockId);
            if (order.Buy == false && walletState == null)
            {
                order.Status = "Cancelled - not enough stocks";
                order.StatusEnum = OrderStatusEnum.Cancelled;
                order.Active = false;
                return order;
            }
            if (walletState != null)
            {
                if(order.Buy == false && walletState.Amount < transaction.Amount)
                {
                    order.Status = "Cancelled - not enough stocks";
                    order.StatusEnum = OrderStatusEnum.Cancelled;
                    order.Active = false;
                    return order;
                }
                int newAmount = order.Buy ? walletState.Amount + transaction.Amount : walletState.Amount - transaction.Amount;
                if (newAmount <= 0)
                {
                    _context.WalletsState.Remove(walletState);
                } 
                else
                {
                    walletState.AveragePrice =
                        (
                        (walletState.AveragePrice * walletState.Amount) + transaction.Price
                        ) /
                        (walletState.Amount + transaction.Amount);
                    walletState.Amount = newAmount;
                    walletState.Updated = DateTime.Now;
                    _context.WalletsState.Update(walletState);
                }
                _context.SaveChanges();
            }
            else
            {
                    _context.WalletsState.Add(new WalletState()
                {
                    Wallet = wallet,
                    StockId = order.StockId,
                    Amount = transaction.Amount,
                    AveragePrice = transaction.Price / transaction.Amount,
                    Updated = DateTime.Now,
                });
                _context.SaveChanges();
            }
            order.Amount = order.Amount - transaction.Amount;
            if (order.Amount <= 0)
            {
                order.Status = "Success";
                order.StatusEnum = OrderStatusEnum.Success;
                order.Active = false;
            }
            else
            {
                order.Status = "Partially realized";
                order.StatusEnum = OrderStatusEnum.Partially;
            }
            wallet.Balance = order.Buy == true ? wallet.Balance - transaction.Price : wallet.Balance + transaction.Price;
            return order;
        }
    }
}
