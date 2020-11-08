using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface IWebsocketStocksService
    {
        void Update(StockTrade stock);
    }
}
