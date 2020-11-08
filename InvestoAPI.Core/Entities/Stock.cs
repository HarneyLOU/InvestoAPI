using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestoAPI.Core.Entities
{
    public class Stock
    {

        public int CompanyId { get; set; }

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
    }
}
