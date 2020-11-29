﻿using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface IOrderService
    {
        void AddOrder(Order order);
        IEnumerable<Order> GetActiveOrders();
        IEnumerable<Order> GetOrdersFromWallet(int walletId);
        Order GetOrder(int id);
        void DeleteOrder(int id);
        void UpdateOrder(Order order);
    }
}