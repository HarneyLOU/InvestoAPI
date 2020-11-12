using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestoAPI.Core.Entities
{
    public class StockDaily
    {
        public int CompanyId { get; set; }

        public string Symbol { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Open { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal High { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Low { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Close { get; set; }

        public long Volume { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
    }
}
