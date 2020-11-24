using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Services
{
    public class OrderQueueService
    {
        private Queue<Order> ordersQueue;

        public OrderQueueService()
        {
            ordersQueue = new Queue<Order>();
        }

        public bool IsEmpty()
        {
            return ordersQueue.Count == 0;
        }

        public void AddOrder(Order order)
        {
            ordersQueue.Enqueue(order);
        }

        public Order GetOrder()
        {
            return ordersQueue.Peek();
        }

        public Order FinishOrder()
        {
            return ordersQueue.Dequeue();
        }
    }
}
