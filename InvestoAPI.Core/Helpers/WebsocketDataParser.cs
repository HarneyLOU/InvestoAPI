using InvestoAPI.Core.Helpers;
using InvestoAPI.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvestoAPI.Core.Helpers
{
    public static class WebsocketDataParser
    {
        public static TradeStock Parse(string data, WebsocketDataType type)
        {
            switch (type)
            {
                case WebsocketDataType.Tiingo:
                        return ParseTiingo(data);
                default:
                    return null;
            }
        }

        private static TradeStock ParseTiingo(string data)
        {
            if(!String.IsNullOrEmpty(data)) { 
            JsonDocument document = JsonDocument.Parse(data);
                if (document.RootElement.TryGetProperty("messageType", out JsonElement messageType))
                {
                    if (messageType.GetString().Equals("A"))
                    {
                        document.RootElement.TryGetProperty("data", out JsonElement stockData);
                        if (stockData[0].GetString().Equals("T"))
                        {
                            var symbol = stockData[3].GetString().ToUpper();
                            var price = stockData[9].GetDecimal();
                            var date = stockData[1].GetDateTime();
                            return new TradeStock
                            {
                                Symbol = symbol,
                                Price = price,
                                Date = date,
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
